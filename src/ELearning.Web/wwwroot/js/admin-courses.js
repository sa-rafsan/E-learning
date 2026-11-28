$(function () {
    let coursesCache = [];
    let teacherOptions = [];
    let studentOptions = [];
    let currentManageCourseId = null;
    let currentCourseBatches = [];

    const createCourseModalEl = document.getElementById("createCourseModal");
    const editCourseModalEl = document.getElementById("editCourseModal");
    const manageCourseModalEl = document.getElementById("manageCourseModal");

    const createCourseModal = createCourseModalEl ? new bootstrap.Modal(createCourseModalEl) : null;
    const editCourseModal = editCourseModalEl ? new bootstrap.Modal(editCourseModalEl) : null;
    const manageCourseModal = manageCourseModalEl ? new bootstrap.Modal(manageCourseModalEl) : null;

    function escapeHtml(text) {
        return $("<div>").text(text ?? "").html();
    }

    function showCourseAlert(message, type) {
        $("#adminAlertContainer").html(`
            <div class="alert alert-${type} alert-dismissible fade show" role="alert">
                <span>${escapeHtml(message)}</span>
                <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
            </div>`);
    }

    function showManageAlert(message, type) {
        $("#manageCourseAlert").html(`
            <div class="alert alert-${type} alert-dismissible fade show py-2 small" role="alert">
                <span>${escapeHtml(message)}</span>
                <button type="button" class="btn-close btn-sm" data-bs-dismiss="alert" aria-label="Close"></button>
            </div>`);
    }

    function loadOptions() {
        return $.get("/api/admin/courses/options").done(function (data) {
            teacherOptions = data.teachers || [];
            studentOptions = data.students || [];

            const teacherOptsHtml = teacherOptions.map(t =>
                `<option value="${t.id}">${escapeHtml(t.fullName)} (${escapeHtml(t.email)})</option>`
            ).join("");

            $("#courseTeacher").html(`<option value="">-- Assign Later --</option>${teacherOptsHtml}`);
            $("#editCourseTeacher").html(`<option value="">-- Assign Later --</option>${teacherOptsHtml}`);
        });
    }

    function renderCourses(courses) {
        const $tbody = $("#coursesTableBody");
        if (!courses.length) {
            $tbody.html('<tr><td colspan="7" class="text-center py-4 text-muted">No courses found.</td></tr>');
            return;
        }

        const rows = courses.map(c => {
            const thumb = c.imagePath
                ? `<img src="${escapeHtml(c.imagePath)}" alt="${escapeHtml(c.title)}" class="rounded object-fit-cover border me-2" style="width:36px;height:28px;" />`
                : "";
            return `
            <tr>
                <td class="fw-semibold">${thumb}${escapeHtml(c.title)}</td>
                <td><span class="badge bg-light text-dark border">${escapeHtml(c.courseCode || "-")}</span> <span class="small text-muted">${c.durationWeeks || 12}w</span></td>
                <td>${escapeHtml(c.category)}</td>
                <td><span class="badge bg-info-subtle text-info-emphasis">${c.batchesCount || 0} batches</span></td>
                <td>${escapeHtml(c.teacherName || "TBA")}</td>
                <td><span class="badge bg-secondary-subtle text-secondary">${c.enrolledStudentsCount} students</span></td>
                <td class="text-end text-nowrap">
                    <button type="button" class="btn btn-sm btn-outline-primary btn-edit-course me-1" data-id="${c.id}">Edit</button>
                    <button type="button" class="btn btn-sm btn-primary btn-manage-course" data-id="${c.id}">Manage</button>
                </td>
            </tr>`;
        });

        $tbody.html(rows.join(""));
    }

    function loadCourses() {
        $.get("/api/admin/courses")
            .done(function (courses) {
                coursesCache = courses;
                $("#statCourses").text(courses.length);
                renderCourses(courses);
            })
            .fail(() => showCourseAlert("Failed to load courses.", "danger"));
    }

    $("#openCreateCourseBtn").on("click", function () {
        window.location.href = "/Admin/CourseForm";
    });

    $("#createCourseForm").on("submit", function (e) {
        e.preventDefault();
        $("#createCourseFormError").addClass("d-none").text("");

        const title = ($("#courseTitle").val() || "").trim();
        const category = ($("#courseCategory").val() || "").trim();
        const description = ($("#courseDescription").val() || "").trim();
        const duration = parseInt($("#courseDuration").val(), 10);
        let valid = true;

        if (!title) { $("#courseTitle").addClass("is-invalid"); $("#courseTitleError").text("Course Title is required."); valid = false; }
        else { $("#courseTitle").removeClass("is-invalid"); }

        if (!category) { $("#courseCategory").addClass("is-invalid"); $("#courseCategoryError").text("Category is required."); valid = false; }
        else { $("#courseCategory").removeClass("is-invalid"); }

        if (!description) { $("#courseDescription").addClass("is-invalid"); $("#courseDescriptionError").text("Description is required."); valid = false; }
        else { $("#courseDescription").removeClass("is-invalid"); }

        if (!duration || duration < 1) { $("#courseDuration").addClass("is-invalid"); $("#courseDurationError").text("Duration must be at least 1 hour."); valid = false; }
        else { $("#courseDuration").removeClass("is-invalid"); }

        if (!valid) return;

        const teacherVal = $("#courseTeacher").val();
        $.ajax({
            url: "/api/admin/courses",
            method: "POST",
            contentType: "application/json",
            data: JSON.stringify({
                title,
                category,
                description,
                durationHours: duration,
                teacherId: teacherVal ? parseInt(teacherVal, 10) : null,
                isPublished: $("#courseIsPublished").is(":checked")
            }),
            success: function (created) {
                createCourseModal.hide();
                showCourseAlert(`Course '${created.title}' created successfully.`, "success");
                loadCourses();
            },
            error: function (xhr) {
                $("#createCourseFormError").removeClass("d-none").text(xhr.responseJSON?.message || "Failed to create course.");
            }
        });
    });

    // --- Edit Course Modal ---
    $(document).on("click", ".btn-edit-course", function () {
        const id = $(this).data("id");
        $("#editCourseFormError").addClass("d-none").text("");
        $("#editCourseForm .is-invalid").removeClass("is-invalid");

        $.when(loadOptions(), $.get(`/api/admin/courses/${id}`))
            .done(function (_, courseRes) {
                const c = courseRes[0];
                $("#editCourseId").val(c.id);
                $("#editCourseTitle").val(c.title || "");
                $("#editCourseCode").val(c.courseCode || "");
                $("#editCourseCategory").val(c.category || "");
                $("#editCourseDurationWeeks").val(c.durationWeeks || 12);
                $("#editCourseDurationHours").val(c.durationHours || 12);
                $("#editCourseTeacher").val(c.teacherId || "");
                $("#editCourseDescription").val(c.description || "");
                $("#editCourseIsPublished").prop("checked", !!c.isPublished);
                editCourseModal.show();
            })
            .fail(function () {
                showCourseAlert("Failed to load course details.", "danger");
            });
    });

    $("#editCourseForm").on("submit", function (e) {
        e.preventDefault();
        $("#editCourseFormError").addClass("d-none").text("");

        const id = $("#editCourseId").val();
        const title = ($("#editCourseTitle").val() || "").trim();
        const courseCode = ($("#editCourseCode").val() || "").trim() || null;
        const category = ($("#editCourseCategory").val() || "").trim();
        const durationWeeks = parseInt($("#editCourseDurationWeeks").val(), 10) || 12;
        const durationHours = parseInt($("#editCourseDurationHours").val(), 10);
        const description = ($("#editCourseDescription").val() || "").trim();
        const teacherVal = $("#editCourseTeacher").val();

        let valid = true;
        if (!title) { $("#editCourseTitle").addClass("is-invalid"); $("#editCourseTitleError").text("Course Title is required."); valid = false; }
        else { $("#editCourseTitle").removeClass("is-invalid"); }

        if (!category) { $("#editCourseCategory").addClass("is-invalid"); $("#editCourseCategoryError").text("Category is required."); valid = false; }
        else { $("#editCourseCategory").removeClass("is-invalid"); }

        if (!durationHours || durationHours < 1) { $("#editCourseDurationHours").addClass("is-invalid"); $("#editCourseDurationHoursError").text("Duration must be at least 1 hour."); valid = false; }
        else { $("#editCourseDurationHours").removeClass("is-invalid"); }

        if (!description) { $("#editCourseDescription").addClass("is-invalid"); $("#editCourseDescriptionError").text("Description is required."); valid = false; }
        else { $("#editCourseDescription").removeClass("is-invalid"); }

        if (!valid) return;

        $.ajax({
            url: `/api/admin/courses/${id}`,
            method: "PUT",
            contentType: "application/json",
            data: JSON.stringify({
                title,
                courseCode,
                category,
                durationWeeks,
                durationHours,
                description,
                teacherId: teacherVal ? parseInt(teacherVal, 10) : null,
                isPublished: $("#editCourseIsPublished").is(":checked")
            }),
            success: function (updated) {
                editCourseModal.hide();
                showCourseAlert(`Course '${updated.title}' updated successfully.`, "success");
                loadCourses();
            },
            error: function (xhr) {
                $("#editCourseFormError").removeClass("d-none").text(xhr.responseJSON?.message || "Failed to update course.");
            }
        });
    });

    // --- Manage Course Modal (Row-wise Teacher, Multi-Batch, Searchable Students Workspace) ---
    function getAvailableBatches() {
        const masterBatches = (window.AdminMasterData && window.AdminMasterData.getBatches()) || [];
        return masterBatches.map(mb => ({
            courseBatchId: null,
            masterBatchId: mb.id,
            name: mb.name,
            code: mb.code
        }));
    }

    function buildTeacherOptionsHtml(selectedTeacherId) {
        const opts = teacherOptions.map(t =>
            `<option value="${t.id}" ${String(t.id) === String(selectedTeacherId) ? "selected" : ""}>${escapeHtml(t.fullName)}</option>`
        ).join("");
        return `<option value="">-- Select Teacher --</option>${opts}`;
    }

    function buildBatchDropdownMenuHtml(selectedCodes) {
        const selectedSet = new Set((selectedCodes || []).map(String));
        const batches = getAvailableBatches();
        if (!batches.length) {
            return '<div class="text-muted small p-1">No batches found.</div>';
        }
        return batches.map((b, idx) => `
            <div class="form-check py-1">
                <input class="form-check-input row-batch-cb" type="checkbox" value="${escapeHtml(b.code)}" data-master-batch-id="${b.masterBatchId || ""}" data-name="${escapeHtml(b.name)}" data-cb-id="${b.courseBatchId || ""}" id="rb_${Date.now()}_${idx}" ${selectedSet.has(b.code) ? "checked" : ""} />
                <label class="form-check-label small w-100" for="rb_${Date.now()}_${idx}">${escapeHtml(b.name)} (${escapeHtml(b.code)})</label>
            </div>`).join("");
    }

    function buildSectionDropdownMenuHtml(selectedSections) {
        const selectedSet = new Set((selectedSections || []).map(String));
        const sections = (window.AdminMasterData && window.AdminMasterData.getSections && window.AdminMasterData.getSections()) || [
            { id: 1, name: "A" }, { id: 2, name: "B" }, { id: 3, name: "C" }, { id: 4, name: "D" }
        ];
        return sections.map((s, idx) => `
            <div class="form-check py-1">
                <input class="form-check-input row-section-cb" type="checkbox" value="${escapeHtml(s.name)}" data-sec-id="${s.id}" id="rsec_${Date.now()}_${idx}" ${selectedSet.has(s.name) ? "checked" : ""} />
                <label class="form-check-label small w-100" for="rsec_${Date.now()}_${idx}">Section ${escapeHtml(s.name)}</label>
            </div>`).join("");
    }

    function buildStudentDropdownMenuHtml(selectedStudentIds) {
        const selectedSet = new Set((selectedStudentIds || []).map(String));
        const items = studentOptions.map((s, idx) => {
            const isHidden = idx >= 20 && !selectedSet.has(String(s.id)) ? "d-none" : "";
            return `
            <div class="form-check py-1 row-student-item ${isHidden}" data-idx="${idx}" data-search="${escapeHtml((s.studentId + " " + s.fullName).toLowerCase())}">
                <input class="form-check-input row-student-cb" type="checkbox" value="${s.id}" data-batch="${escapeHtml(s.batch || "")}" data-section="${escapeHtml(s.section || "")}" id="rs_${Date.now()}_${idx}" ${selectedSet.has(String(s.id)) ? "checked" : ""} />
                <label class="form-check-label small w-100" for="rs_${Date.now()}_${idx}">${escapeHtml(s.studentId)} - ${escapeHtml(s.fullName)}</label>
            </div>`;
        }).join("");
        return `
            <input type="search" class="form-control form-control-sm mb-2 row-student-search" placeholder="Search student ID or name..." />
            <div class="row-student-list">${items || '<div class="text-muted small">No students found.</div>'}</div>`;
    }

    function createAssignmentRowHtml(batchCodes, teacherId, studentIds, firstCourseBatchId, selectedSections) {
        const batchLabel = (batchCodes && batchCodes.length) ? batchCodes.join(", ") : "Select Batch(es)...";
        const secLabel = (selectedSections && selectedSections.length) ? selectedSections.join(", ") : "Select Section(s)...";
        const stuLabel = (studentIds && studentIds.length) ? `${studentIds.length} student(s)` : "Select Students...";
        return `
            <tr class="manage-assignment-row">
                <td>
                    <select class="form-select form-select-sm row-teacher-select">
                        ${buildTeacherOptionsHtml(teacherId)}
                    </select>
                </td>
                <td>
                    <div class="dropdown">
                        <button class="btn btn-sm btn-outline-secondary bg-white text-dark dropdown-toggle w-100 text-start d-flex justify-content-between align-items-center" type="button" data-bs-toggle="dropdown" data-bs-auto-close="outside">
                            <span class="row-batch-label text-truncate">${escapeHtml(batchLabel)}</span>
                        </button>
                        <div class="dropdown-menu p-2 shadow-sm w-100" style="min-width: 220px; max-height: 220px; overflow-y: auto;">
                            ${buildBatchDropdownMenuHtml(batchCodes)}
                        </div>
                    </div>
                </td>
                <td>
                    <div class="dropdown">
                        <button class="btn btn-sm btn-outline-secondary bg-white text-dark dropdown-toggle w-100 text-start d-flex justify-content-between align-items-center" type="button" data-bs-toggle="dropdown" data-bs-auto-close="outside">
                            <span class="row-section-label text-truncate">${escapeHtml(secLabel)}</span>
                        </button>
                        <div class="dropdown-menu p-2 shadow-sm w-100" style="min-width: 160px; max-height: 220px; overflow-y: auto;">
                            ${buildSectionDropdownMenuHtml(selectedSections)}
                        </div>
                    </div>
                </td>
                <td>
                    <div class="dropdown">
                        <button class="btn btn-sm btn-outline-secondary bg-white text-dark dropdown-toggle w-100 text-start d-flex justify-content-between align-items-center" type="button" data-bs-toggle="dropdown" data-bs-auto-close="outside">
                            <span class="row-student-label text-truncate">${escapeHtml(stuLabel)}</span>
                        </button>
                        <div class="dropdown-menu p-2 shadow-sm w-100" style="min-width: 260px; max-height: 260px; overflow-y: auto;">
                            ${buildStudentDropdownMenuHtml(studentIds)}
                        </div>
                    </div>
                </td>
            </tr>`;
    }

    function renderManageBatchesTable() {
        $("#manageRunningBatchesCount").text(`${currentCourseBatches.length} running`);
        const $tbody = $("#manageBatchesTableBody");
        if (!currentCourseBatches.length) {
            $tbody.html('<tr><td colspan="7" class="text-center py-3 text-muted small">No batches created for this course yet.</td></tr>');
            return;
        }
        const editIcon = `<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>`;
        const deleteIcon = `<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><polyline points="3 6 5 6 21 6"/><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"/></svg>`;
        const rows = currentCourseBatches.map(b => {
            const tNames = (b.teachers || []).map(t => escapeHtml(t.fullName)).join(", ") || "TBA";
            return `
                <tr>
                    <td class="fw-semibold">${escapeHtml(b.batchName)}</td>
                    <td><span class="badge bg-light text-dark border">${escapeHtml(b.batchCode)}</span></td>
                    <td class="small text-muted">${escapeHtml(b.schedule || "-")}</td>
                    <td class="small text-muted">${escapeHtml(b.startDate)} to ${escapeHtml(b.endDate)}</td>
                    <td class="small">${tNames}</td>
                    <td><span class="badge bg-secondary-subtle text-secondary">${b.enrolledCount || 0} students</span></td>
                    <td class="text-end text-nowrap">
                        <button type="button" class="btn btn-sm btn-outline-primary btn-edit-course-batch me-1" data-batch-id="${b.id}" title="Edit Batch" aria-label="Edit Batch">${editIcon}</button>
                        <button type="button" class="btn btn-sm btn-outline-danger btn-delete-course-batch me-1" data-batch-id="${b.id}" title="Delete Batch" aria-label="Delete Batch">${deleteIcon}</button>
                        <a href="/Workspace/Course/${currentManageCourseId}?batchId=${b.id}" class="btn btn-sm btn-outline-success">Workspace</a>
                    </td>
                </tr>`;
        });
        $tbody.html(rows.join(""));
    }

    function renderManageAssignmentRows() {
        $("#manageAssignmentRowsBody").html(createAssignmentRowHtml([], "", [], null, []));
    }

    function loadManageCourseData(courseId) {
        return $.get(`/api/admin/batches?courseId=${courseId}`).done(function (batches) {
            currentCourseBatches = batches || [];
            renderManageBatchesTable();
            renderManageAssignmentRows();
        });
    }

    $(document).on("click", ".btn-manage-course", function () {
        const id = $(this).data("id");
        const course = coursesCache.find(c => c.id === id);
        if (!course) return;

        currentManageCourseId = course.id;
        $("#manageCourseId").val(course.id);
        $("#manageCourseTitle").text(course.title);
        $("#manageCourseWorkspaceLink").attr("href", `/Workspace/Course/${course.id}`);
        $("#manageCourseAlert").empty();

        const today = new Date().toISOString().split("T")[0];
        $("#manageBatchStartDate").val(today);
        $("#manageBatchEndDate").val(today);

        loadOptions().done(function () {
            loadManageCourseData(course.id).done(function () {
                manageCourseModal.show();
            });
        });
    });

    $("#addManageRowBtn").on("click", function () {
        const $tbody = $("#manageAssignmentRowsBody");
        if ($tbody.find(".manage-assignment-row").length === 0) {
            $tbody.empty();
        }
        $tbody.append(createAssignmentRowHtml([], "", [], null));
    });

    $(document).on("change", ".row-batch-cb", function () {
        const $row = $(this).closest("tr");
        const selected = [];
        $row.find(".row-batch-cb:checked").each(function () { selected.push($(this).val()); });
        $row.find(".row-batch-label").text(selected.length ? selected.join(", ") : "Select Batch(es)...");

        if ($("#manageEditCourseBatchId").val() && !$(this).is(":checked")) {
            const code = ($(this).val() || "").toLowerCase();
            const name = (($(this).data("name") || "") + "").toLowerCase();
            $row.find(".row-student-cb:checked").each(function () {
                const sb = (($(this).data("batch") || "") + "").toLowerCase();
                if (sb && (sb === code || sb === name || name.includes(sb))) {
                    $(this).prop("checked", false);
                }
            });
            const count = $row.find(".row-student-cb:checked").length;
            $row.find(".row-student-label").text(count ? `${count} student(s)` : "Select Students...");
        }
    });

    $(document).on("change", ".row-student-cb", function () {
        const $row = $(this).closest("tr");
        const count = $row.find(".row-student-cb:checked").length;
        $row.find(".row-student-label").text(count ? `${count} student(s)` : "Select Students...");
    });

    $(document).on("change", ".row-section-cb", function () {
        const $row = $(this).closest("tr");
        const selected = [];
        $row.find(".row-section-cb:checked").each(function () { selected.push($(this).val()); });
        $row.find(".row-section-label").text(selected.length ? selected.join(", ") : "Select Section(s)...");

        if ($("#manageEditCourseBatchId").val() && !$(this).is(":checked")) {
            const sec = ($(this).val() || "").toLowerCase();
            $row.find(".row-student-cb:checked").each(function () {
                const ss = (($(this).data("section") || "") + "").toLowerCase();
                if (ss && ss === sec) {
                    $(this).prop("checked", false);
                }
            });
            const count = $row.find(".row-student-cb:checked").length;
            $row.find(".row-student-label").text(count ? `${count} student(s)` : "Select Students...");
        }
    });

    $(document).on("input", ".row-student-search", function () {
        const term = ($(this).val() || "").trim().toLowerCase();
        let shown = 0;
        $(this).closest(".dropdown-menu").find(".row-student-item").each(function () {
            const text = $(this).data("search") || "";
            const isChecked = $(this).find(".row-student-cb").is(":checked");
            if (!term) {
                const idx = parseInt($(this).data("idx"), 10);
                $(this).toggleClass("d-none", idx >= 20 && !isChecked);
            } else {
                const match = text.includes(term);
                const shouldShow = match && shown < 20;
                if (shouldShow) shown++;
                $(this).toggleClass("d-none", !shouldShow && !isChecked);
            }
        });
    });

    $(document).on("click", ".btn-remove-manage-row", function () {
        $(this).closest("tr").remove();
    });

    $(document).on("click", ".btn-save-manage-row", function () {
        const $row = $(this).closest("tr");
        const teacherId = $row.find(".row-teacher-select").val();
        const selectedBatches = [];
        $row.find(".row-batch-cb:checked").each(function () {
            selectedBatches.push({
                code: $(this).val(),
                name: $(this).data("name"),
                courseBatchId: $(this).data("cb-id")
            });
        });
        const studentIds = [];
        $row.find(".row-student-cb:checked").each(function () {
            studentIds.push(parseInt($(this).val(), 10));
        });

        if (!selectedBatches.length) {
            showManageAlert("Please select at least one Batch before saving.", "warning");
            return;
        }

        const teacherIds = teacherId ? [parseInt(teacherId, 10)] : [];
        const today = new Date().toISOString().split("T")[0];

        const requests = selectedBatches.map(b => {
            if (b.courseBatchId) {
                return $.when(
                    $.ajax({ url: `/api/admin/batches/${b.courseBatchId}/teachers`, method: "PUT", contentType: "application/json", data: JSON.stringify({ teacherIds }) }),
                    $.ajax({ url: `/api/admin/batches/${b.courseBatchId}/students`, method: "POST", contentType: "application/json", data: JSON.stringify({ studentIds, batch: null, section: null }) })
                );
            }
            return $.ajax({
                url: "/api/admin/batches",
                method: "POST",
                contentType: "application/json",
                data: JSON.stringify({
                    courseId: currentManageCourseId,
                    batchName: b.name,
                    batchCode: b.code,
                    schedule: null,
                    startDate: today,
                    endDate: today,
                    status: 1,
                    teacherIds
                })
            }).then(created => {
                if (studentIds.length && created && created.id) {
                    return $.ajax({
                        url: `/api/admin/batches/${created.id}/students`,
                        method: "POST",
                        contentType: "application/json",
                        data: JSON.stringify({ studentIds, batch: null, section: null })
                    });
                }
            });
        });

        $.when.apply($, requests)
            .done(function () {
                showManageAlert("Teacher, batch(es), and student assignments saved.", "success");
                loadManageCourseData(currentManageCourseId);
                loadCourses();
            })
            .fail(function (xhr) {
                showManageAlert(xhr?.responseJSON?.message || "Failed to save row assignments.", "danger");
            });
    });

    function computeSmartStudentIds() {
        const $row = $("#manageAssignmentRowsBody .manage-assignment-row").first();
        const selectedBatches = [];
        $row.find(".row-batch-cb:checked").each(function () {
            selectedBatches.push({
                code: ($(this).val() || "").toLowerCase(),
                name: (($(this).data("name") || "") + "").toLowerCase()
            });
        });
        const selectedSections = [];
        $row.find(".row-section-cb:checked").each(function () {
            selectedSections.push(($(this).val() || "").toLowerCase());
        });
        const extraStudentIds = [];
        $row.find(".row-student-cb:checked").each(function () {
            extraStudentIds.push(parseInt($(this).val(), 10));
        });

        const matchedIds = new Set(extraStudentIds);
        if (selectedBatches.length > 0) {
            studentOptions.forEach(s => {
                const sBatch = (s.batch || "").toLowerCase();
                const sDept = (s.department || "").toLowerCase();
                const sSec = (s.section || "").toLowerCase();
                const batchMatches = selectedBatches.some(b =>
                    (sBatch && (sBatch === b.code || sBatch === b.name || b.name.includes(sBatch))) ||
                    (sDept && (sDept.includes(b.code) || sDept.includes(b.name)))
                );
                if (!batchMatches) return;
                if (selectedSections.length === 0 || selectedSections.includes(sSec)) {
                    matchedIds.add(s.id);
                }
            });
        }
        return Array.from(matchedIds);
    }

    function resetManageBatchForm() {
        $("#manageEditCourseBatchId").val("");
        $("#manageCreateBatchForm")[0].reset();
        const today = new Date().toISOString().split("T")[0];
        $("#manageBatchStartDate").val(today);
        $("#manageBatchEndDate").val(today);
        $("#manageCreateBatchSubmitBtn").text("+ Create Batch");
        $("#manageCancelEditBatchBtn").addClass("d-none");
        renderManageAssignmentRows();
    }

    $(document).on("click", ".btn-edit-course-batch", function () {
        const bId = $(this).data("batch-id");
        const b = currentCourseBatches.find(x => x.id === bId);
        if (!b) return;
        $("#manageEditCourseBatchId").val(b.id);
        $("#manageBatchNameInput").val(b.batchName);
        $("#manageBatchCodeInput").val(b.batchCode);
        $("#manageBatchScheduleInput").val(b.schedule || "");
        $("#manageBatchStartDate").val(b.startDate);
        $("#manageBatchEndDate").val(b.endDate);
        $("#manageCreateBatchSubmitBtn").text("Update Batch");
        $("#manageCancelEditBatchBtn").removeClass("d-none");

        const firstTeacherId = (b.teachers && b.teachers.length) ? b.teachers[0].id : "";
        const masterBatches = (window.AdminMasterData && window.AdminMasterData.getBatches()) || [];
        const matchedMb = masterBatches.find(mb => mb.id === b.masterBatchId || mb.code === b.batchCode);
        const batchCodes = matchedMb ? [matchedMb.code] : [];
        const sectionNames = b.sectionNames || [];
        $("#manageAssignmentRowsBody").html(createAssignmentRowHtml(batchCodes, firstTeacherId, b.studentIds || [], b.id, sectionNames));
    });

    $("#manageCancelEditBatchBtn").on("click", resetManageBatchForm);

    $(document).on("click", ".btn-delete-course-batch", function () {
        const bId = $(this).data("batch-id");
        $.ajax({
            url: `/api/admin/batches/${bId}`,
            method: "DELETE",
            success: function (res) {
                showManageAlert(res.message || "Batch deleted.", "info");
                loadManageCourseData(currentManageCourseId);
                loadCourses();
            },
            error: function (xhr) {
                showManageAlert(xhr.responseJSON?.message || "Failed to delete batch.", "danger");
            }
        });
    });

    $("#manageCreateBatchForm").on("submit", function (e) {
        e.preventDefault();
        if (!currentManageCourseId) return;

        const editBatchId = $("#manageEditCourseBatchId").val();
        const $row = $("#manageAssignmentRowsBody .manage-assignment-row").first();
        const teacherVal = $row.find(".row-teacher-select").val();
        const teacherIds = teacherVal ? [parseInt(teacherVal, 10)] : [];
        const studentIds = computeSmartStudentIds();

        const firstBatchCb = $row.find(".row-batch-cb:checked").first();
        const masterBatchId = firstBatchCb.length && firstBatchCb.data("master-batch-id")
            ? parseInt(firstBatchCb.data("master-batch-id"), 10)
            : null;
        const masterSectionIds = [];
        $row.find(".row-section-cb:checked").each(function () {
            const sid = parseInt($(this).data("sec-id"), 10);
            if (!isNaN(sid)) masterSectionIds.push(sid);
        });

        if (editBatchId) {
            $.when(
                $.ajax({ url: `/api/admin/batches/${editBatchId}/teachers`, method: "PUT", contentType: "application/json", data: JSON.stringify({ teacherIds }) }),
                $.ajax({ url: `/api/admin/batches/${editBatchId}/students`, method: "POST", contentType: "application/json", data: JSON.stringify({ studentIds, batch: null, section: null }) }),
                $.ajax({ url: `/api/admin/batches/${editBatchId}/master-refs`, method: "PUT", contentType: "application/json", data: JSON.stringify({ masterBatchId, masterSectionIds }) })
            ).done(function () {
                resetManageBatchForm();
                loadManageCourseData(currentManageCourseId);
                loadCourses();
                showManageAlert(`Batch updated with ${studentIds.length} student(s) in database.`, "success");
            }).fail(function (xhr) {
                showManageAlert(xhr?.responseJSON?.message || "Failed to update batch.", "danger");
            });
            return;
        }

        $.ajax({
            url: "/api/admin/batches",
            method: "POST",
            contentType: "application/json",
            data: JSON.stringify({
                courseId: currentManageCourseId,
                batchName: $("#manageBatchNameInput").val(),
                batchCode: $("#manageBatchCodeInput").val(),
                schedule: $("#manageBatchScheduleInput").val() || null,
                startDate: $("#manageBatchStartDate").val(),
                endDate: $("#manageBatchEndDate").val(),
                status: 1,
                teacherIds,
                masterBatchId,
                masterSectionIds
            }),
            success: function (created) {
                const enrollReq = (studentIds.length && created && created.id)
                    ? $.ajax({
                        url: `/api/admin/batches/${created.id}/students`,
                        method: "POST",
                        contentType: "application/json",
                        data: JSON.stringify({ studentIds, batch: null, section: null })
                    })
                    : $.Deferred().resolve();

                enrollReq.done(function () {
                    resetManageBatchForm();
                    loadManageCourseData(currentManageCourseId);
                    loadCourses();
                    showManageAlert(`Batch created and ${studentIds.length} student(s) enrolled in database.`, "success");
                });
            },
            error: xhr => showManageAlert(xhr.responseJSON?.message || "Failed to create batch.", "danger")
        });
    });

    $("#refreshCoursesBtn").on("click", loadCourses);

    loadOptions();
    loadCourses();
});
