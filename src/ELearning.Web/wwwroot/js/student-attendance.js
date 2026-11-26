$(function () {
    let coursesCache = [];
    const lessonsModal = new bootstrap.Modal(document.getElementById("courseLessonsModal"));

    const gaugeThemes = [
        { card: "gauge-theme-blue", stroke: "#2563eb", text: "#1d4ed8" },
        { card: "gauge-theme-green", stroke: "#10b981", text: "#047857" },
        { card: "gauge-theme-amber", stroke: "#f59e0b", text: "#d97706" },
        { card: "gauge-theme-purple", stroke: "#9333ea", text: "#7e22ce" },
        { card: "gauge-theme-indigo", stroke: "#4f46e5", text: "#4338ca" }
    ];

    function escapeHtml(text) {
        return $("<div>").text(text ?? "").html();
    }

    function buildCircularGauge(percentage, strokeColor, textColor) {
        const r = 28;
        const c = 2 * Math.PI * r;
        const dash = (percentage / 100) * c;

        return `
            <svg width="72" height="72" viewBox="0 0 72 72" class="mb-2">
                <circle cx="36" cy="36" r="${r}" fill="none" stroke="#e2e8f0" stroke-width="6" />
                <circle cx="36" cy="36" r="${r}" fill="none" stroke="${strokeColor}" stroke-width="6"
                        stroke-linecap="round" stroke-dasharray="${dash} ${c}" transform="rotate(-90 36 36)" />
                <text x="36" y="40" text-anchor="middle" font-size="13" font-weight="700" fill="${textColor}">${percentage}%</text>
            </svg>`;
    }

    function renderCourseGauges(courses) {
        coursesCache = courses;
        $("#courseProgressCountBadge").text(`${courses.length} Courses`);

        if (!courses.length) {
            $("#courseGaugesContainer").html('<div class="text-muted small py-4">No courses assigned yet.</div>');
            return;
        }

        const html = courses.map((c, idx) => {
            const theme = gaugeThemes[idx % gaugeThemes.length];
            return `
                <div class="gauge-card ${theme.card}" data-course-id="${c.courseId}">
                    ${buildCircularGauge(c.percentage, theme.stroke, theme.text)}
                    <div class="fw-bold text-uppercase small mb-1 text-truncate" style="color:${theme.text};" title="${escapeHtml(c.title)}">
                        ${escapeHtml(c.title)}
                    </div>
                    <div class="mb-3">
                        <span class="fw-bold fs-5 text-dark">${c.completedLessons}</span>
                        <span class="text-muted small">/ ${c.totalLessons}</span>
                    </div>
                    <div class="d-flex justify-content-between pt-2 border-top small text-muted" style="font-size:0.74rem;">
                        <span>Left: ${c.remainingLessons}</span>
                        <span>Done: ${c.completedLessons}</span>
                    </div>
                </div>`;
        });

        $("#courseGaugesContainer").html(html.join(""));
    }

    function loadCourseProgress() {
        $.get("/api/student/progress")
            .done(renderCourseGauges)
            .fail(() => $("#courseGaugesContainer").html('<div class="text-danger small py-4">Unable to load course progress.</div>'));
    }

    function renderLessonChecklist(course) {
        $("#courseLessonsModalLabel").text(`${course.title} (${course.percentage}%)`);
        if (!course.lessons || !course.lessons.length) {
            $("#courseLessonsModalBody").html('<div class="text-muted small">No lessons in this course yet.</div>');
            return;
        }

        const html = course.lessons.map(l => `
            <div class="form-check py-2 border-bottom">
                <input class="form-check-input toggle-lesson-checkbox" type="checkbox"
                       value="${l.lessonId}" id="lessonChk_${l.lessonId}" ${l.isCompleted ? "checked" : ""} />
                <label class="form-check-label d-flex justify-content-between align-items-center" for="lessonChk_${l.lessonId}">
                    <span>${l.order}. ${escapeHtml(l.title)}</span>
                    <span class="text-muted small">${l.durationMinutes} min</span>
                </label>
            </div>`);
        $("#courseLessonsModalBody").html(html.join(""));
    }

    $(document).on("click", ".gauge-card", function () {
        const courseId = $(this).data("course-id");
        const course = coursesCache.find(c => c.courseId === courseId);
        if (!course) return;
        renderLessonChecklist(course);
        lessonsModal.show();
    });

    $(document).on("change", ".toggle-lesson-checkbox", function () {
        const lessonId = $(this).val();
        $.post(`/api/student/progress/lessons/${lessonId}/toggle`)
            .done(function () {
                $.get("/api/student/progress").done(function (courses) {
                    renderCourseGauges(courses);
                    const updated = courses.find(c => c.lessons.some(l => l.lessonId == lessonId));
                    if (updated) {
                        $("#courseLessonsModalLabel").text(`${updated.title} (${updated.percentage}%)`);
                    }
                });
            });
    });

    function statusBadge(status) {
        switch (status) {
            case "Present": return "bg-success-subtle text-success";
            case "Absent": return "bg-danger-subtle text-danger";
            case "Late": return "bg-warning-subtle text-warning-emphasis";
            default: return "bg-info-subtle text-info-emphasis";
        }
    }

    $.get("/api/attendance/my")
        .done(function (records) {
            if (!records.length) {
                $("#studentAttendanceBody").html('<tr><td colspan="6" class="text-center py-3 text-muted">No attendance records yet.</td></tr>');
                return;
            }

            const rows = records.map(r => `
                <tr>
                    <td>${escapeHtml(r.sessionDate)}</td>
                    <td class="fw-semibold">${escapeHtml(r.courseTitle)}</td>
                    <td>${escapeHtml(r.teacherName)}</td>
                    <td>${escapeHtml(r.topic || "-")}</td>
                    <td><span class="badge ${statusBadge(r.status)}">${escapeHtml(r.status)}</span></td>
                    <td>${escapeHtml(r.remarks || "-")}</td>
                </tr>`);

            $("#studentAttendanceBody").html(rows.join(""));
        })
        .fail(function () {
            $("#studentAttendanceBody").html('<tr><td colspan="6" class="text-center py-3 text-danger">Unable to load attendance records.</td></tr>');
        });

    loadCourseProgress();
});
