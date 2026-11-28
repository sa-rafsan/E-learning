$(function () {
    const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    let allUsersCache = [];

    const createTeacherModalEl = document.getElementById("createTeacherModal");
    const editTeacherModalEl = document.getElementById("editTeacherModal");
    const editStudentModalEl = document.getElementById("editStudentModal");
    const viewUserModalEl = document.getElementById("viewUserModal");
    const resetPasswordModalEl = document.getElementById("resetPasswordModal");

    const createTeacherModal = createTeacherModalEl ? new bootstrap.Modal(createTeacherModalEl) : null;
    const editTeacherModal = editTeacherModalEl ? new bootstrap.Modal(editTeacherModalEl) : null;
    const editStudentModal = editStudentModalEl ? new bootstrap.Modal(editStudentModalEl) : null;
    const viewUserModal = viewUserModalEl ? new bootstrap.Modal(viewUserModalEl) : null;
    const resetPasswordModal = resetPasswordModalEl ? new bootstrap.Modal(resetPasswordModalEl) : null;

    let toastTimer = null;
    function showToast(title, message, isError) {
        const icon = isError
            ? '<span class="text-danger fw-bold">&#9888;</span>'
            : '<span class="text-success fw-bold">&#10003;</span>';

        $("#adminToastContainer").html(`
            <div class="admin-toast-card ${isError ? "" : "toast-success"}" role="alert">
                <div>${icon}</div>
                <div class="flex-grow-1">
                    <div class="fw-bold small text-dark">${escapeHtml(title)}</div>
                    <div class="small text-muted">${escapeHtml(message)}</div>
                </div>
                <button type="button" class="btn-close btn-sm" id="closeAdminToastBtn" aria-label="Close"></button>
            </div>`);

        clearTimeout(toastTimer);
        toastTimer = setTimeout(() => $("#adminToastContainer").empty(), 4500);
    }

    $(document).on("click", "#closeAdminToastBtn", () => $("#adminToastContainer").empty());

    function showAlert(message, type) {
        const alertHtml = `
            <div class="alert alert-${type} alert-dismissible fade show" role="alert">
                <span>${escapeHtml(message)}</span>
                <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
            </div>`;
        $("#adminAlertContainer").html(alertHtml);
        showToast(type === "danger" ? "Action Failed" : "Success", message, type === "danger");
    }

    function escapeHtml(text) {
        return $("<div>").text(text ?? "").html();
    }

    function setFieldError($input, $error, message) {
        $input.addClass("is-invalid");
        $error.text(message);
    }

    function clearFieldError($input, $error) {
        $input.removeClass("is-invalid");
        $error.text("");
    }

    function validateRequired($input, $error, label) {
        const val = ($input.val() || "").trim();
        if (!val) {
            setFieldError($input, $error, `${label} is required.`);
            return false;
        }
        clearFieldError($input, $error);
        return true;
    }

    function validateEmail($input, $error) {
        const val = ($input.val() || "").trim();
        if (!val) {
            setFieldError($input, $error, "Email is required.");
            return false;
        }
        if (!emailPattern.test(val)) {
            setFieldError($input, $error, "Please enter a valid email address.");
            return false;
        }
        clearFieldError($input, $error);
        return true;
    }

    function validatePasswordPair($pwd, $pwdErr, $confirm, $confirmErr) {
        const pwd = $pwd.val() || "";
        const confirm = $confirm.val() || "";
        let valid = true;

        if (!pwd) {
            setFieldError($pwd, $pwdErr, "Password is required.");
            valid = false;
        } else if (pwd.length < 8) {
            setFieldError($pwd, $pwdErr, "Password must be at least 8 characters.");
            valid = false;
        } else {
            clearFieldError($pwd, $pwdErr);
        }

        if (!confirm) {
            setFieldError($confirm, $confirmErr, "Confirm Password is required.");
            valid = false;
        } else if (confirm !== pwd) {
            setFieldError($confirm, $confirmErr, "Confirm Password must match Password.");
            valid = false;
        } else {
            clearFieldError($confirm, $confirmErr);
        }

        return valid;
    }

    function clearFormValidation($form) {
        $form.find(".is-invalid").removeClass("is-invalid");
        $form.find(".invalid-feedback").text("");
        $form.find(".alert-danger").addClass("d-none").text("");
    }

    function extractApiError(xhr, fallback) {
        const resp = xhr.responseJSON;
        if (!resp) return fallback;
        if (resp.errors) {
            const firstKey = Object.keys(resp.errors)[0];
            if (firstKey && resp.errors[firstKey] && resp.errors[firstKey].length > 0) {
                return resp.errors[firstKey][0];
            }
        }
        return resp.message || fallback;
    }

    function updateStats(users) {
        const teachersCount = users.filter(u => u.role === "Teacher").length;
        const studentsCount = users.filter(u => u.role === "Student").length;
        const adminCount = users.filter(u => u.role === "Admin").length;

        $("#statTotalUsers").text(users.length);
        $("#statTeachers").text(teachersCount);
        $("#statStudents").text(studentsCount);
        $("#statActiveUsers").text(users.filter(u => u.status === "Active").length);

        renderSectionBarChart(users);
        renderRoleDonutChart(studentsCount, teachersCount, adminCount);
    }

    function renderSectionBarChart(users) {
        const sections = ["A", "B", "C", "D"];
        const counts = sections.map(sec =>
            users.filter(u => u.role === "Student" && (u.department || "").includes(`Section ${sec}`)).length
        );
        const maxVal = Math.max(1, ...counts);

        const barsHtml = sections.map((sec, idx) => {
            const pct = Math.round((counts[idx] / maxVal) * 145);
            return `
                <div class="bar-col">
                    <span class="bar-count">${counts[idx]}</span>
                    <div class="bar-fill" style="height: ${Math.max(8, pct)}px;"></div>
                    <span class="bar-label">Section ${sec}</span>
                </div>`;
        }).join("");

        $("#sectionBarChart").html(barsHtml);
    }

    function renderRoleDonutChart(students, teachers, admins) {
        const total = Math.max(1, students + teachers + admins);
        const r = 56;
        const c = 2 * Math.PI * r;
        const stuLen = (students / total) * c;
        const teaLen = (teachers / total) * c;
        const admLen = (admins / total) * c;

        const svg = `
            <svg width="160" height="160" viewBox="0 0 160 160" aria-label="Role distribution chart">
                <circle cx="80" cy="80" r="${r}" fill="none" stroke="#e2e8f0" stroke-width="22" />
                <circle cx="80" cy="80" r="${r}" fill="none" stroke="#2563eb" stroke-width="22"
                        stroke-dasharray="${stuLen} ${c}" transform="rotate(-90 80 80)" />
                <circle cx="80" cy="80" r="${r}" fill="none" stroke="#ec4899" stroke-width="22"
                        stroke-dasharray="${teaLen} ${c}" stroke-dashoffset="${-stuLen}" transform="rotate(-90 80 80)" />
                <circle cx="80" cy="80" r="${r}" fill="none" stroke="#10b981" stroke-width="22"
                        stroke-dasharray="${admLen} ${c}" stroke-dashoffset="${-(stuLen + teaLen)}" transform="rotate(-90 80 80)" />
            </svg>`;

        $("#roleDonutChart").html(svg);
        $("#roleDonutLegend").html(`
            <span><span style="color:#2563eb;">&#9632;</span> Students (${students})</span>
            <span><span style="color:#ec4899;">&#9632;</span> Teachers (${teachers})</span>
            <span><span style="color:#10b981;">&#9632;</span> Admin (${admins})</span>
        `);
    }

    $(".admin-nav-link[data-section]").on("click", function () {
        $(".admin-nav-link").removeClass("active");
        $(this).addClass("active");
        $("#sectionAddStudent, #sectionOverview, #sectionCharts, #sectionCourses, #sectionTeachers, #sectionStudents, #sectionBatchesDepartments").addClass("d-none");
        const sec = $(this).data("section");
        if (sec === "overview") {
            $("#sectionOverview, #sectionCharts").removeClass("d-none");
        } else if (sec === "teachers") {
            $("#sectionTeachers").removeClass("d-none");
        } else if (sec === "students") {
            $("#sectionStudents").removeClass("d-none");
        } else if (sec === "courses") {
            $("#sectionCourses").removeClass("d-none");
        } else if (sec === "batches-departments") {
            $("#sectionBatchesDepartments").removeClass("d-none");
        }
    });

    function showAddStudentSection() {
        const $form = $("#createStudentForm");
        $form[0].reset();
        clearFormValidation($form);
        $("#studentStatus").val("1");
        $(".status-pill-btn").removeClass("active-status inactive-status");
        $(".status-pill-btn[data-status-val='1']").addClass("active-status");
        $(".admin-nav-link").removeClass("active");
        $(".admin-nav-link[data-section='students']").addClass("active");
        $("#sectionOverview, #sectionCharts, #sectionCourses, #sectionTeachers, #sectionStudents").addClass("d-none");
        $("#sectionAddStudent").removeClass("d-none");
    }

    function showDirectorySection() {
        $("#sectionAddStudent").addClass("d-none");
        $(".admin-nav-link").removeClass("active");
        $(".admin-nav-link[data-section='students']").addClass("active");
        $("#sectionStudents").removeClass("d-none");
    }

    $(".status-pill-btn").on("click", function () {
        const val = $(this).data("status-val").toString();
        $("#studentStatus").val(val);
        $(".status-pill-btn").removeClass("active-status inactive-status");
        $(this).addClass(val === "1" ? "active-status" : "inactive-status");
    });

    $("#backToDirectoryBtn, #cancelAddStudentBtn").on("click", showDirectorySection);
    $("#resetAddStudentBtn").on("click", function () {
        const $form = $("#createStudentForm");
        $form[0].reset();
        clearFormValidation($form);
        $("#studentStatus").val("1");
        $(".status-pill-btn").removeClass("active-status inactive-status");
        $(".status-pill-btn[data-status-val='1']").addClass("active-status");
    });
    $("#sidebarAddTeacherBtn, #openCreateTeacherBtn").on("click", () => { window.location.href = "/Admin/TeacherForm"; });
    $("#sidebarAddStudentBtn, #openCreateStudentBtn").on("click", () => { window.location.href = "/Admin/StudentForm"; });
    $("#sidebarAddCourseBtn").on("click", () => { window.location.href = "/Admin/CourseForm"; });

    const initialHash = (window.location.hash || "").replace("#", "");
    if (["teachers", "students", "courses"].includes(initialHash)) {
        $(`.admin-nav-link[data-section='${initialHash}']`).trigger("click");
    }

    let currentPage = 1;
    const avatarColors = [
        { bg: "#dbeafe", fg: "#1d4ed8" },
        { bg: "#fce7f3", fg: "#be185d" },
        { bg: "#dcfce7", fg: "#15803d" },
        { bg: "#fef3c7", fg: "#b45309" },
        { bg: "#f3e8ff", fg: "#7e22ce" }
    ];

    function getInitials(name) {
        const parts = (name || "U").trim().split(/\s+/);
        return parts.length >= 2
            ? (parts[0][0] + parts[1][0]).toUpperCase()
            : parts[0].substring(0, 2).toUpperCase();
    }

    function applyDirectoryFiltersAndRender() {
        const search = ($("#userSearchInput").val() || "").trim().toLowerCase();
        const sectionFilter = $("#userSectionFilter").val() || "";
        const statusFilter = $("#userStatusFilter").val() || "";
        const pageSize = parseInt($("#directoryPageSize").val(), 10) || 10;

        let filtered = allUsersCache.filter(u => u.role === "Student");
        if (search) {
            filtered = filtered.filter(u =>
                (u.fullName || "").toLowerCase().includes(search) ||
                (u.email || "").toLowerCase().includes(search) ||
                (u.studentId || "").toLowerCase().includes(search) ||
                (u.phoneNumber || "").toLowerCase().includes(search));
        }
        if (sectionFilter) {
            filtered = filtered.filter(u => (u.department || "").includes(sectionFilter));
        }
        if (statusFilter) {
            filtered = filtered.filter(u => u.status === statusFilter);
        }

        const total = filtered.length;
        $("#directoryTotalCount").text(total);

        const totalPages = Math.max(1, Math.ceil(total / pageSize));
        if (currentPage > totalPages) currentPage = totalPages;

        const startIdx = (currentPage - 1) * pageSize;
        const pageItems = filtered.slice(startIdx, startIdx + pageSize);

        renderUsers(pageItems);
        renderPagination(total, totalPages, startIdx, pageItems.length);
        renderTeachersList();
    }

    function renderTeachersList() {
        const search = ($("#teacherSearchInput").val() || "").trim().toLowerCase();
        const statusFilter = $("#teacherStatusFilter").val() || "";

        let teachers = allUsersCache.filter(u => u.role === "Teacher");
        if (search) {
            teachers = teachers.filter(t =>
                (t.fullName || "").toLowerCase().includes(search) ||
                (t.email || "").toLowerCase().includes(search) ||
                (t.phoneNumber || "").toLowerCase().includes(search));
        }
        if (statusFilter) {
            teachers = teachers.filter(t => t.status === statusFilter);
        }

        $("#teachersTotalCount").text(teachers.length);
        const $tbody = $("#teachersTableBody");
        if (!teachers.length) {
            $tbody.html('<tr><td colspan="5" class="text-center py-4 text-muted">No teachers found.</td></tr>');
            return;
        }

        const rows = teachers.map((u, idx) => {
            const palette = avatarColors[(u.id || idx) % avatarColors.length];
            const statusBadge = u.status === "Active" ? "bg-success-subtle text-success" : "bg-secondary-subtle text-secondary";
            const createdStr = new Date(u.createdAtUtc).toLocaleDateString(undefined, { month: "short", day: "numeric", year: "numeric" });
            const nextStatus = u.status === "Active" ? 2 : 1;
            const toggleTitle = u.status === "Active" ? "Deactivate" : "Activate";

            const avatarHtml = u.imagePath
                ? `<img src="${escapeHtml(u.imagePath)}" alt="${escapeHtml(u.fullName)}" class="rounded-circle object-fit-cover border" style="width:38px;height:38px;flex-shrink:0;" />`
                : `<span class="dir-avatar" style="background-color:${palette.bg};color:${palette.fg};">${escapeHtml(getInitials(u.fullName))}</span>`;

            return `
                <tr>
                    <td>
                        <div class="d-flex align-items-center gap-3">
                            ${avatarHtml}
                            <div>
                                <div class="fw-semibold text-dark">${escapeHtml(u.fullName)}</div>
                                <div class="small text-muted">Faculty</div>
                            </div>
                        </div>
                    </td>
                    <td>
                        <div class="small text-dark">${escapeHtml(u.email)}</div>
                        <div class="small text-muted">${escapeHtml(u.phoneNumber || "-")}</div>
                    </td>
                    <td><span class="badge ${statusBadge} rounded-pill px-2 py-1">&bull; ${escapeHtml(u.status)}</span></td>
                    <td class="small text-muted">${createdStr}</td>
                    <td class="text-end text-nowrap">
                        <button type="button" class="dir-action-btn dir-action-view btn-view-user" data-id="${u.id}" title="View Details" aria-label="View Details">
                            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z" /><circle cx="12" cy="12" r="3" /></svg>
                        </button>
                        <button type="button" class="dir-action-btn dir-action-edit btn-edit-user" data-id="${u.id}" title="Edit Teacher" aria-label="Edit Teacher">
                            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7" /><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z" /></svg>
                        </button>
                        <button type="button" class="dir-action-btn dir-action-toggle btn-toggle-status" data-id="${u.id}" data-status="${nextStatus}" title="${toggleTitle}" aria-label="${toggleTitle}">
                            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M18.36 6.64a9 9 0 1 1-12.73 0" /><line x1="12" y1="2" x2="12" y2="12" /></svg>
                        </button>
                        <button type="button" class="dir-action-btn dir-action-reset btn-reset-pwd" data-id="${u.id}" title="Reset Password" aria-label="Reset Password">
                            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="11" width="18" height="11" rx="2" ry="2" /><path d="M7 11V7a5 5 0 0 1 10 0v4" /></svg>
                        </button>
                    </td>
                </tr>`;
        });
        $tbody.html(rows.join(""));
    }

    function renderPagination(total, totalPages, startIdx, count) {
        if (total === 0) {
            $("#directoryPageInfo").text("Showing 0 of 0");
            $("#directoryPagination").html("");
            return;
        }

        $("#directoryPageInfo").text(`Showing ${startIdx + 1}–${startIdx + count} of ${total}`);

        let html = `<li class="page-item ${currentPage === 1 ? "disabled" : ""}">
            <button type="button" class="page-link" data-page="${currentPage - 1}">Prev</button>
        </li>`;

        const maxVisible = 5;
        let startPage = Math.max(1, currentPage - 2);
        let endPage = Math.min(totalPages, startPage + maxVisible - 1);
        if (endPage - startPage < maxVisible - 1) {
            startPage = Math.max(1, endPage - maxVisible + 1);
        }

        for (let p = startPage; p <= endPage; p++) {
            html += `<li class="page-item ${p === currentPage ? "active" : ""}">
                <button type="button" class="page-link" data-page="${p}">${p}</button>
            </li>`;
        }

        html += `<li class="page-item ${currentPage === totalPages ? "disabled" : ""}">
            <button type="button" class="page-link" data-page="${currentPage + 1}">Next</button>
        </li>`;

        $("#directoryPagination").html(html);
    }

    function renderUsers(users) {
        const $tbody = $("#usersTableBody");
        if (!users.length) {
            $tbody.html('<tr><td colspan="6" class="text-center py-4 text-muted">No users found.</td></tr>');
            return;
        }

        const rows = users.map((u, idx) => {
            const palette = avatarColors[(u.id || idx) % avatarColors.length];
            const roleBadge = u.role === "Admin"
                ? "bg-dark text-white"
                : u.role === "Teacher"
                    ? "bg-primary-subtle text-primary"
                    : "bg-info-subtle text-info-emphasis";

            const statusBadge = u.status === "Active"
                ? "bg-success-subtle text-success"
                : "bg-secondary-subtle text-secondary";

            const subId = u.role === "Student" && u.studentId ? u.studentId : `${u.role} Account`;
            const deptText = u.department || (u.role === "Teacher" ? "Faculty" : "Administration");
            const createdStr = new Date(u.createdAtUtc).toLocaleDateString(undefined, { month: "short", day: "numeric", year: "numeric" });

            const isAdmin = u.role === "Admin";
            const nextStatus = u.status === "Active" ? 2 : 1;
            const toggleTitle = u.status === "Active" ? "Deactivate" : "Activate";

            const avatarHtml = u.imagePath
                ? `<img src="${escapeHtml(u.imagePath)}" alt="${escapeHtml(u.fullName)}" class="rounded-circle object-fit-cover border" style="width:38px;height:38px;flex-shrink:0;" />`
                : `<span class="dir-avatar" style="background-color:${palette.bg};color:${palette.fg};">${escapeHtml(getInitials(u.fullName))}</span>`;

            return `
                <tr>
                    <td>
                        <div class="d-flex align-items-center gap-3">
                            ${avatarHtml}
                            <div>
                                <div class="fw-semibold text-dark">${escapeHtml(u.fullName)}</div>
                                <div class="small text-muted">${escapeHtml(subId)}</div>
                            </div>
                        </div>
                    </td>
                    <td>
                        <span class="badge ${roleBadge} rounded-pill mb-1">${escapeHtml(u.role)}</span>
                        <div class="small text-muted">${escapeHtml(deptText)}</div>
                    </td>
                    <td>
                        <div class="small text-dark">${escapeHtml(u.email)}</div>
                        <div class="small text-muted">${escapeHtml(u.phoneNumber || "-")}</div>
                    </td>
                    <td>
                        <span class="badge ${statusBadge} rounded-pill px-2 py-1">&bull; ${escapeHtml(u.status)}</span>
                    </td>
                    <td class="small text-muted">${createdStr}</td>
                    <td class="text-end text-nowrap">
                        <button type="button" class="dir-action-btn dir-action-view btn-view-user" data-id="${u.id}" title="View Details" aria-label="View Details">
                            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z" /><circle cx="12" cy="12" r="3" /></svg>
                        </button>
                        ${!isAdmin ? `
                            <button type="button" class="dir-action-btn dir-action-edit btn-edit-user" data-id="${u.id}" title="Edit User" aria-label="Edit User">
                                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7" /><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z" /></svg>
                            </button>
                            <button type="button" class="dir-action-btn dir-action-toggle btn-toggle-status" data-id="${u.id}" data-status="${nextStatus}" title="${toggleTitle}" aria-label="${toggleTitle}">
                                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M18.36 6.64a9 9 0 1 1-12.73 0" /><line x1="12" y1="2" x2="12" y2="12" /></svg>
                            </button>
                            <button type="button" class="dir-action-btn dir-action-reset btn-reset-pwd" data-id="${u.id}" title="Reset Password" aria-label="Reset Password">
                                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="11" width="18" height="11" rx="2" ry="2" /><path d="M7 11V7a5 5 0 0 1 10 0v4" /></svg>
                            </button>
                        ` : ""}
                    </td>
                </tr>`;
        });

        $tbody.html(rows.join(""));
    }

    function loadUsers() {
        const search = ($("#userSearchInput").val() || "").trim();
        const role = $("#userRoleFilter").val() || "";
        const params = new URLSearchParams();
        if (search) params.set("search", search);
        if (role) params.set("role", role);

        $.get("/api/admin/users?" + params.toString())
            .done(function (users) {
                allUsersCache = users;
                currentPage = 1;
                applyDirectoryFiltersAndRender();
                if (!search && !role) {
                    updateStats(users);
                }
            })
            .fail(function (xhr) {
                if (xhr.status === 401) {
                    window.location.href = "/Account/Login";
                    return;
                }
                showAlert("Failed to load users.", "danger");
            });
    }

    // Bind live input clearing for Create Teacher
    $("#teacherFullName").on("input blur", () => validateRequired($("#teacherFullName"), $("#teacherFullNameError"), "Full Name"));
    $("#teacherEmail").on("input blur", () => validateEmail($("#teacherEmail"), $("#teacherEmailError")));
    $("#teacherPassword, #teacherConfirmPassword").on("input blur", () =>
        validatePasswordPair($("#teacherPassword"), $("#teacherPasswordError"), $("#teacherConfirmPassword"), $("#teacherConfirmPasswordError")));

    function validateMinLength($input, $error, label, minLen) {
        const val = ($input.val() || "").trim();
        if (!val) {
            setFieldError($input, $error, `${label} is required.`);
            return false;
        }
        if (val.length < minLen) {
            setFieldError($input, $error, `${label} must be at least ${minLen} characters.`);
            return false;
        }
        clearFieldError($input, $error);
        return true;
    }

    // Bind live input clearing for Create Student
    $("#studentFullName").on("input blur", () => validateMinLength($("#studentFullName"), $("#studentFullNameError"), "Full Name", 2));
    $("#studentEmail").on("input blur", () => validateEmail($("#studentEmail"), $("#studentEmailError")));
    $("#studentIdInput").on("input blur", () => validateRequired($("#studentIdInput"), $("#studentIdInputError"), "Student ID"));
    $("#studentPhone").on("input blur", () => validateRequired($("#studentPhone"), $("#studentPhoneError"), "Phone Number"));
    $("#studentPassword, #studentConfirmPassword").on("input blur", () =>
        validatePasswordPair($("#studentPassword"), $("#studentPasswordError"), $("#studentConfirmPassword"), $("#studentConfirmPasswordError")));

    // Open Create Teacher Modal
    $("#openCreateTeacherBtn").on("click", function () {
        const $form = $("#createTeacherForm");
        $form[0].reset();
        clearFormValidation($form);
        createTeacherModal.show();
    });

    // Submit Create Teacher
    $("#createTeacherForm").on("submit", function (e) {
        e.preventDefault();
        $("#createTeacherFormError").addClass("d-none").text("");

        const v1 = validateRequired($("#teacherFullName"), $("#teacherFullNameError"), "Full Name");
        const v2 = validateEmail($("#teacherEmail"), $("#teacherEmailError"));
        const v3 = validatePasswordPair($("#teacherPassword"), $("#teacherPasswordError"), $("#teacherConfirmPassword"), $("#teacherConfirmPasswordError"));
        if (!v1 || !v2 || !v3) return;

        $.ajax({
            url: "/api/admin/users/teacher",
            method: "POST",
            contentType: "application/json",
            data: JSON.stringify({
                fullName: ($("#teacherFullName").val() || "").trim(),
                email: ($("#teacherEmail").val() || "").trim(),
                phoneNumber: ($("#teacherPhone").val() || "").trim() || null,
                password: $("#teacherPassword").val(),
                confirmPassword: $("#teacherConfirmPassword").val(),
                status: parseInt($("#teacherStatus").val(), 10)
            }),
            success: function (created) {
                createTeacherModal.hide();
                showAlert(`Teacher '${created.fullName}' created successfully.`, "success");
                loadUsers();
            },
            error: function (xhr) {
                $("#createTeacherFormError").removeClass("d-none").text(extractApiError(xhr, "Failed to create teacher."));
            }
        });
    });

    // Submit Create Student
    $("#createStudentForm").on("submit", function (e) {
        e.preventDefault();
        $("#createStudentFormError").addClass("d-none").text("");

        const v1 = validateMinLength($("#studentFullName"), $("#studentFullNameError"), "Full Name", 2);
        const v2 = validateEmail($("#studentEmail"), $("#studentEmailError"));
        const v3 = validateRequired($("#studentIdInput"), $("#studentIdInputError"), "Student ID");
        const v4 = validateRequired($("#studentPhone"), $("#studentPhoneError"), "Phone Number");
        const v5 = validatePasswordPair($("#studentPassword"), $("#studentPasswordError"), $("#studentConfirmPassword"), $("#studentConfirmPasswordError"));
        if (!v1 || !v2 || !v3 || !v4 || !v5) {
            showToast("Form Validation Error", "Please correct the highlighted fields before saving.", true);
            return;
        }

        $.ajax({
            url: "/api/admin/users/student",
            method: "POST",
            contentType: "application/json",
            data: JSON.stringify({
                fullName: ($("#studentFullName").val() || "").trim(),
                email: ($("#studentEmail").val() || "").trim(),
                phoneNumber: ($("#studentPhone").val() || "").trim() || null,
                studentId: ($("#studentIdInput").val() || "").trim(),
                department: ($("#studentDepartment").val() || "").trim() || null,
                password: $("#studentPassword").val(),
                confirmPassword: $("#studentConfirmPassword").val(),
                status: parseInt($("#studentStatus").val(), 10)
            }),
            success: function (created) {
                showAlert(`Student '${created.fullName}' created successfully.`, "success");
                showDirectorySection();
                loadUsers();
            },
            error: function (xhr) {
                $("#createStudentFormError").removeClass("d-none").text(extractApiError(xhr, "Failed to create student."));
            }
        });
    });

    // View User Details
    $(document).on("click", ".btn-view-user", function () {
        const id = $(this).data("id");
        $.get(`/api/admin/users/${id}`)
            .done(function (u) {
                const initials = (u.fullName || "U").split(" ").map(p => p[0]).join("").substring(0, 2).toUpperCase();
                const avatarHtml = u.imagePath
                    ? `<img src="${escapeHtml(u.imagePath)}" alt="${escapeHtml(u.fullName)}" class="rounded-circle object-fit-cover border border-2 border-white shadow-sm" style="width:72px;height:72px;" />`
                    : `<div class="rounded-circle bg-primary text-white d-flex align-items-center justify-content-center fw-bold fs-4 border border-2 border-white shadow-sm" style="width:72px;height:72px;">${escapeHtml(initials)}</div>`;
                const statusBadge = u.status === "Active"
                    ? `<span class="badge rounded-pill bg-success-subtle text-success px-3 py-1">&#9679; Active ${escapeHtml(u.role)}</span>`
                    : `<span class="badge rounded-pill bg-warning-subtle text-warning-emphasis px-3 py-1">&#9679; ${escapeHtml(u.status)}</span>`;
                const editHref = u.role === "Teacher" ? `/Admin/TeacherForm/${u.id}` : `/Admin/StudentForm/${u.id}`;
                const createdDate = new Date(u.createdAtUtc).toLocaleDateString(undefined, { year: "numeric", month: "short", day: "numeric" });

                $("#viewUserModalBody").html(`
                    <div class="rounded-3 p-4 mb-4 text-white d-flex flex-wrap justify-content-between align-items-center gap-3" style="background: linear-gradient(135deg, #0f172a 0%, #1e293b 100%);">
                        <div class="d-flex align-items-center gap-3">
                            ${avatarHtml}
                            <div>
                                <h3 class="h4 fw-bold mb-1 text-white">${escapeHtml(u.fullName)}</h3>
                                <div class="small text-white-50 mb-2">${escapeHtml(u.studentId || u.role)}</div>
                                <div class="d-flex flex-wrap gap-2">
                                    ${u.department ? `<span class="badge rounded-pill bg-light text-dark px-3 py-1">${escapeHtml(u.department)}</span>` : ""}
                                    ${statusBadge}
                                </div>
                            </div>
                        </div>
                        <div class="d-flex gap-2">
                            <button type="button" class="btn btn-sm btn-light" onclick="window.print()">Print</button>
                            <a href="${editHref}" class="btn btn-sm btn-primary">Edit Profile</a>
                        </div>
                    </div>

                    <div class="row g-3 mb-3">
                        <div class="col-md-6">
                            <div class="card border-0 shadow-sm h-100">
                                <div class="card-body">
                                    <h4 class="h6 fw-bold mb-3 pb-2 border-bottom">${u.role === "Student" ? "Academic Details" : "Faculty Details"}</h4>
                                    <div class="d-flex justify-content-between py-2 border-bottom small">
                                        <span class="text-muted">Role</span>
                                        <span class="fw-semibold">${escapeHtml(u.role)}</span>
                                    </div>
                                    ${u.role === "Student" ? `
                                    <div class="d-flex justify-content-between py-2 border-bottom small">
                                        <span class="text-muted">Student ID</span>
                                        <span class="fw-semibold">${escapeHtml(u.studentId || "-")}</span>
                                    </div>
                                    <div class="d-flex justify-content-between py-2 border-bottom small">
                                        <span class="text-muted">Department</span>
                                        <span class="fw-semibold">${escapeHtml(u.department || "-")}</span>
                                    </div>` : ""}
                                    <div class="d-flex justify-content-between py-2 small">
                                        <span class="text-muted">Record Created</span>
                                        <span class="fw-semibold">${escapeHtml(createdDate)}</span>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="col-md-6">
                            <div class="card border-0 shadow-sm h-100">
                                <div class="card-body">
                                    <h4 class="h6 fw-bold mb-3 pb-2 border-bottom">Personal &amp; Account Status</h4>
                                    <div class="d-flex justify-content-between py-2 border-bottom small">
                                        <span class="text-muted">Full Name</span>
                                        <span class="fw-semibold">${escapeHtml(u.fullName)}</span>
                                    </div>
                                    <div class="d-flex justify-content-between py-2 border-bottom small">
                                        <span class="text-muted">Account Status</span>
                                        <span class="fw-semibold">${escapeHtml(u.status)}</span>
                                    </div>
                                    <div class="d-flex justify-content-between py-2 small">
                                        <span class="text-muted">User ID</span>
                                        <span class="fw-semibold">#${u.id}</span>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="card border-0 shadow-sm">
                        <div class="card-body">
                            <h4 class="h6 fw-bold mb-3 pb-2 border-bottom">Contact &amp; Residence Information</h4>
                            <div class="d-flex justify-content-between py-2 border-bottom small">
                                <span class="text-muted">Email Address</span>
                                <a href="mailto:${escapeHtml(u.email)}" class="fw-semibold text-decoration-none">${escapeHtml(u.email)}</a>
                            </div>
                            <div class="d-flex justify-content-between py-2 border-bottom small">
                                <span class="text-muted">Telephone / Mobile</span>
                                <span class="fw-semibold">${escapeHtml(u.phoneNumber || "-")}</span>
                            </div>
                            <div class="d-flex justify-content-between py-2 small">
                                <span class="text-muted">Residential Address</span>
                                <span class="fw-semibold">${escapeHtml(u.address || "-")}</span>
                            </div>
                        </div>
                    </div>
                `);
                viewUserModal.show();
            })
            .fail(() => showAlert("Unable to load user details.", "danger"));
    });

    // Open Edit User on dedicated Add/Edit page
    $(document).on("click", ".btn-edit-user", function () {
        const id = $(this).data("id");
        const user = allUsersCache.find(u => u.id === id);
        if (!user) return;

        if (user.role === "Teacher") {
            window.location.href = `/Admin/TeacherForm/${id}`;
        } else if (user.role === "Student") {
            window.location.href = `/Admin/StudentForm/${id}`;
        }
    });

    // Submit Edit Teacher
    $("#editTeacherForm").on("submit", function (e) {
        e.preventDefault();
        $("#editTeacherFormError").addClass("d-none").text("");

        const v1 = validateRequired($("#editTeacherFullName"), $("#editTeacherFullNameError"), "Full Name");
        const v2 = validateEmail($("#editTeacherEmail"), $("#editTeacherEmailError"));
        if (!v1 || !v2) return;

        const id = $("#editTeacherId").val();
        $.ajax({
            url: `/api/admin/users/teacher/${id}`,
            method: "PUT",
            contentType: "application/json",
            data: JSON.stringify({
                fullName: ($("#editTeacherFullName").val() || "").trim(),
                email: ($("#editTeacherEmail").val() || "").trim(),
                phoneNumber: ($("#editTeacherPhone").val() || "").trim() || null,
                status: parseInt($("#editTeacherStatus").val(), 10)
            }),
            success: function (updated) {
                editTeacherModal.hide();
                showAlert(`Teacher '${updated.fullName}' updated successfully.`, "success");
                loadUsers();
            },
            error: function (xhr) {
                $("#editTeacherFormError").removeClass("d-none").text(extractApiError(xhr, "Failed to update teacher."));
            }
        });
    });

    // Submit Edit Student
    $("#editStudentForm").on("submit", function (e) {
        e.preventDefault();
        $("#editStudentFormError").addClass("d-none").text("");

        const v1 = validateRequired($("#editStudentFullName"), $("#editStudentFullNameError"), "Full Name");
        const v2 = validateEmail($("#editStudentEmail"), $("#editStudentEmailError"));
        const v3 = validateRequired($("#editStudentCode"), $("#editStudentCodeError"), "Student ID");
        if (!v1 || !v2 || !v3) return;

        const id = $("#editStudentId").val();
        $.ajax({
            url: `/api/admin/users/student/${id}`,
            method: "PUT",
            contentType: "application/json",
            data: JSON.stringify({
                fullName: ($("#editStudentFullName").val() || "").trim(),
                email: ($("#editStudentEmail").val() || "").trim(),
                studentId: ($("#editStudentCode").val() || "").trim(),
                department: ($("#editStudentDepartment").val() || "").trim() || null,
                phoneNumber: ($("#editStudentPhone").val() || "").trim() || null,
                status: parseInt($("#editStudentStatus").val(), 10)
            }),
            success: function (updated) {
                editStudentModal.hide();
                showAlert(`Student '${updated.fullName}' updated successfully.`, "success");
                loadUsers();
            },
            error: function (xhr) {
                $("#editStudentFormError").removeClass("d-none").text(extractApiError(xhr, "Failed to update student."));
            }
        });
    });

    // Toggle Status
    $(document).on("click", ".btn-toggle-status", function () {
        const id = $(this).data("id");
        const status = $(this).data("status");

        $.ajax({
            url: `/api/admin/users/${id}/status`,
            method: "PUT",
            contentType: "application/json",
            data: JSON.stringify({ status }),
            success: function (updated) {
                showAlert(`Account '${updated.fullName}' is now ${updated.status}.`, "success");
                loadUsers();
            },
            error: function (xhr) {
                showAlert(extractApiError(xhr, "Failed to update status."), "danger");
            }
        });
    });

    // Open Reset Password
    $(document).on("click", ".btn-reset-pwd", function () {
        const id = $(this).data("id");
        const user = allUsersCache.find(u => u.id === id);
        if (!user) return;

        const $form = $("#resetPasswordForm");
        $form[0].reset();
        clearFormValidation($form);
        $("#resetPasswordUserId").val(user.id);
        $("#resetPasswordUserName").text(user.fullName);
        resetPasswordModal.show();
    });

    $("#resetNewPassword, #resetConfirmPassword").on("input blur", () =>
        validatePasswordPair($("#resetNewPassword"), $("#resetNewPasswordError"), $("#resetConfirmPassword"), $("#resetConfirmPasswordError")));

    // Submit Reset Password
    $("#resetPasswordForm").on("submit", function (e) {
        e.preventDefault();
        $("#resetPasswordFormError").addClass("d-none").text("");

        const valid = validatePasswordPair($("#resetNewPassword"), $("#resetNewPasswordError"), $("#resetConfirmPassword"), $("#resetConfirmPasswordError"));
        if (!valid) return;

        const id = $("#resetPasswordUserId").val();
        $.ajax({
            url: `/api/admin/users/${id}/reset-password`,
            method: "POST",
            contentType: "application/json",
            data: JSON.stringify({
                newPassword: $("#resetNewPassword").val(),
                confirmPassword: $("#resetConfirmPassword").val()
            }),
            success: function () {
                resetPasswordModal.hide();
                showAlert("Password reset successfully.", "success");
            },
            error: function (xhr) {
                $("#resetPasswordFormError").removeClass("d-none").text(extractApiError(xhr, "Failed to reset password."));
            }
        });
    });

    $("#userSearchInput").on("input", function () {
        currentPage = 1;
        applyDirectoryFiltersAndRender();
    });
    $("#userSectionFilter, #userStatusFilter, #directoryPageSize").on("change", function () {
        currentPage = 1;
        applyDirectoryFiltersAndRender();
    });
    $("#resetDirectoryFiltersBtn").on("click", function () {
        $("#userSearchInput").val("");
        $("#userSectionFilter").val("");
        $("#userStatusFilter").val("");
        currentPage = 1;
        applyDirectoryFiltersAndRender();
    });
    $("#teacherSearchInput").on("input", renderTeachersList);
    $("#teacherStatusFilter").on("change", renderTeachersList);
    $("#resetTeacherFiltersBtn").on("click", function () {
        $("#teacherSearchInput").val("");
        $("#teacherStatusFilter").val("");
        renderTeachersList();
    });
    $(document).on("click", "#directoryPagination .page-link", function () {
        const page = parseInt($(this).data("page"), 10);
        if (page > 0) {
            currentPage = page;
            applyDirectoryFiltersAndRender();
        }
    });

    loadUsers();
});

