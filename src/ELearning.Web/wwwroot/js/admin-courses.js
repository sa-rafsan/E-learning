$(function () {
    let coursesCache = [];
    let teacherOptions = [];
    let studentOptions = [];

    const createCourseModal = new bootstrap.Modal(document.getElementById("createCourseModal"));
    const assignTeacherModal = new bootstrap.Modal(document.getElementById("assignTeacherModal"));
    const assignStudentsModal = new bootstrap.Modal(document.getElementById("assignStudentsModal"));

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

    function loadOptions() {
        return $.get("/api/admin/courses/options").done(function (data) {
            teacherOptions = data.teachers || [];
            studentOptions = data.students || [];

            const teacherOptsHtml = teacherOptions.map(t =>
                `<option value="${t.id}">${escapeHtml(t.fullName)} (${escapeHtml(t.email)})</option>`
            ).join("");

            $("#courseTeacher").html(`<option value="">-- Assign Later --</option>${teacherOptsHtml}`);
            $("#assignTeacherSelect").html(teacherOptsHtml);
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
                <td>${escapeHtml(c.category)}</td>
                <td>${c.durationHours}h</td>
                <td>${escapeHtml(c.teacherName || "TBA")}</td>
                <td><span class="badge bg-secondary-subtle text-secondary">${c.enrolledStudentsCount} students</span></td>
                <td>
                    <span class="badge ${c.isPublished ? "bg-success-subtle text-success" : "bg-warning-subtle text-warning-emphasis"}">
                        ${c.isPublished ? "Published" : "Draft"}
                    </span>
                </td>
                <td class="text-end text-nowrap">
                    <a href="/Admin/CourseForm/${c.id}" class="btn btn-sm btn-outline-secondary">Edit</a>
                    <button type="button" class="btn btn-sm btn-outline-primary btn-assign-teacher" data-id="${c.id}">Assign Teacher</button>
                    <button type="button" class="btn btn-sm btn-outline-dark btn-assign-students" data-id="${c.id}">Assign Students</button>
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

    function renderStudentCheckboxes(enrolledIds, filterText) {
        const term = (filterText || "").trim().toLowerCase();
        const enrolledSet = new Set(enrolledIds || []);
        const filtered = term
            ? studentOptions.filter(s =>
                (s.studentId || "").toLowerCase().includes(term) ||
                (s.fullName || "").toLowerCase().includes(term) ||
                (s.email || "").toLowerCase().includes(term))
            : studentOptions;

        if (!filtered.length) {
            $("#enrollStudentsList").html('<div class="text-muted small p-2">No matching students.</div>');
            return;
        }

        const html = filtered.map(s => `
            <div class="form-check py-1 border-bottom">
                <input class="form-check-input enroll-student-checkbox" type="checkbox"
                       value="${s.id}" id="enrollStu_${s.id}" ${enrolledSet.has(s.id) ? "checked" : ""} />
                <label class="form-check-label small" for="enrollStu_${s.id}">
                    <strong>${escapeHtml(s.studentId)}</strong> - ${escapeHtml(s.fullName)}
                    <span class="text-muted">(${escapeHtml(s.email)})</span>
                </label>
            </div>`);
        $("#enrollStudentsList").html(html.join(""));
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

    $(document).on("click", ".btn-assign-teacher", function () {
        const id = $(this).data("id");
        const course = coursesCache.find(c => c.id === id);
        if (!course) return;

        $("#assignTeacherFormError").addClass("d-none").text("");
        $("#assignTeacherCourseId").val(course.id);
        $("#assignTeacherCourseTitle").text(course.title);
        loadOptions().done(function () {
            if (course.teacherId) {
                $("#assignTeacherSelect").val(course.teacherId);
            }
            assignTeacherModal.show();
        });
    });

    $("#assignTeacherForm").on("submit", function (e) {
        e.preventDefault();
        const courseId = $("#assignTeacherCourseId").val();
        const teacherId = parseInt($("#assignTeacherSelect").val(), 10);

        $.ajax({
            url: `/api/admin/courses/${courseId}/teacher`,
            method: "PUT",
            contentType: "application/json",
            data: JSON.stringify({ teacherId }),
            success: function (updated) {
                assignTeacherModal.hide();
                showCourseAlert(`Assigned '${updated.teacherName}' to '${updated.title}'.`, "success");
                loadCourses();
            },
            error: function (xhr) {
                $("#assignTeacherFormError").removeClass("d-none").text(xhr.responseJSON?.message || "Failed to assign teacher.");
            }
        });
    });

    let currentAssignCourse = null;
    $(document).on("click", ".btn-assign-students", function () {
        const id = $(this).data("id");
        currentAssignCourse = coursesCache.find(c => c.id === id);
        if (!currentAssignCourse) return;

        $("#assignStudentsFormError").addClass("d-none").text("");
        $("#assignStudentsCourseId").val(currentAssignCourse.id);
        $("#assignStudentsCourseTitle").text(currentAssignCourse.title);
        $("#enrollBatchInput").val("");
        $("#enrollSectionSelect").val("");
        $("#enrollStudentSearch").val("");

        loadOptions().done(function () {
            renderStudentCheckboxes(currentAssignCourse.enrolledStudentIds, "");
            assignStudentsModal.show();
        });
    });

    $("#enrollStudentSearch").on("input", function () {
        if (!currentAssignCourse) return;
        renderStudentCheckboxes(currentAssignCourse.enrolledStudentIds, $(this).val());
    });

    $("#assignStudentsForm").on("submit", function (e) {
        e.preventDefault();
        const courseId = $("#assignStudentsCourseId").val();
        const batch = ($("#enrollBatchInput").val() || "").trim() || null;
        const section = $("#enrollSectionSelect").val() || null;
        const studentIds = [];
        $(".enroll-student-checkbox:checked").each(function () {
            studentIds.push(parseInt($(this).val(), 10));
        });

        $.ajax({
            url: `/api/admin/courses/${courseId}/students`,
            method: "POST",
            contentType: "application/json",
            data: JSON.stringify({ studentIds, batch, section }),
            success: function (updated) {
                assignStudentsModal.hide();
                showCourseAlert(`Updated enrollments for '${updated.title}' (${updated.enrolledStudentsCount} students).`, "success");
                loadCourses();
            },
            error: function (xhr) {
                $("#assignStudentsFormError").removeClass("d-none").text(xhr.responseJSON?.message || "Failed to assign students.");
            }
        });
    });

    $("#refreshCoursesBtn").on("click", loadCourses);

    loadOptions();
    loadCourses();
});

