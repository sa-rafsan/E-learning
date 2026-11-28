$(function () {
    let coursesCache = [];
    const lessonsModalEl = document.getElementById("courseLessonsModal");
    const lessonsModal = lessonsModalEl ? new bootstrap.Modal(lessonsModalEl) : null;

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
            const total = records.length;
            const presentCount = records.filter(r => r.status === "Present").length;
            const lateCount = records.filter(r => r.status === "Late").length;
            const absentCount = records.filter(r => r.status === "Absent").length;

            const pPct = total ? ((presentCount / total) * 100).toFixed(2) : "0.00";
            const lPct = total ? ((lateCount / total) * 100).toFixed(2) : "0.00";
            const aPct = total ? ((absentCount / total) * 100).toFixed(2) : "0.00";

            $("#stuAttPresentPct").text(`${pPct}%`);
            $("#stuAttPresentPill").text(`${pPct}% (${presentCount})`);
            $("#stuAttLatePill").text(`${lPct}% (${lateCount})`);
            $("#stuAttAbsentPill").text(`${aPct}% (${absentCount})`);

            const circ = 2 * Math.PI * 74;
            const pLen = total ? (presentCount / total) * circ : 0;
            const lLen = total ? (lateCount / total) * circ : 0;
            const aLen = total ? (absentCount / total) * circ : 0;
            $("#stuAttDonutSvg").html(`
                <circle cx="95" cy="95" r="74" fill="none" stroke="#e2e8f0" stroke-width="24" />
                <circle cx="95" cy="95" r="74" fill="none" stroke="#10b981" stroke-width="24" stroke-dasharray="${pLen} ${circ}" transform="rotate(-90 95 95)" />
                <circle cx="95" cy="95" r="74" fill="none" stroke="#f59e0b" stroke-width="24" stroke-dasharray="${lLen} ${circ}" stroke-dashoffset="-${pLen}" transform="rotate(-90 95 95)" />
                <circle cx="95" cy="95" r="74" fill="none" stroke="#ef4444" stroke-width="24" stroke-dasharray="${aLen} ${circ}" stroke-dashoffset="-${pLen + lLen}" transform="rotate(-90 95 95)" />
            `);

            let allAttendanceRecords = records;
            let activeAttFilter = "";

            function renderAttendanceGrid(list) {
                const now = new Date();
                const year = now.getFullYear();
                const month = now.getMonth();
                const firstDay = new Date(year, month, 1).getDay();
                const daysInMonth = new Date(year, month + 1, 0).getDate();
                const recMap = new Map();
                list.forEach(r => recMap.set(r.sessionDate, r));

                const daysHeader = ["SUN", "MON", "TUE", "WED", "THU", "FRI", "SAT"]
                    .map(d => `<div class="text-center small fw-bold text-muted py-2 border-bottom bg-light">${d}</div>`).join("");

                let cells = "";
                for (let i = 0; i < firstDay; i++) {
                    cells += `<div class="border p-2 bg-light-subtle" style="min-height:84px;"></div>`;
                }
                for (let day = 1; day <= daysInMonth; day++) {
                    const mm = String(month + 1).padStart(2, "0");
                    const dd = String(day).padStart(2, "0");
                    const dateStr = `${year}-${mm}-${dd}`;
                    const rec = recMap.get(dateStr);
                    const st = rec ? rec.status : "";
                    const badgeCls = st === "Present" ? "bg-success-subtle text-success"
                        : st === "Late" ? "bg-warning-subtle text-warning-emphasis"
                        : st === "Absent" ? "bg-danger-subtle text-danger"
                        : "bg-light text-muted";
                    cells += `
                        <div class="border p-2 bg-white" style="min-height:84px;">
                            <div class="d-flex justify-content-between align-items-center mb-1">
                                <span class="small fw-bold">${dd}</span>
                                ${st ? `<span class="badge ${badgeCls}" style="font-size:0.65rem;">${escapeHtml(st.toUpperCase())}</span>` : ""}
                            </div>
                            ${rec ? `<div class="text-truncate text-muted" style="font-size:0.7rem;" title="${escapeHtml(rec.courseTitle)}">${escapeHtml(rec.courseTitle)}</div>` : ""}
                        </div>`;
                }
                $("#attendanceGridView").html(`<div style="display:grid;grid-template-columns:repeat(7,1fr);border:1px solid #e2e8f0;border-radius:8px;overflow:hidden;">${daysHeader}${cells}</div>`);
            }

            function renderAttendanceYearly(list) {
                const year = new Date().getFullYear();
                const months = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
                const recMap = new Map();
                list.forEach(r => recMap.set(r.sessionDate, r.status));

                const cards = months.map((mName, mIdx) => {
                    const firstDay = new Date(year, mIdx, 1).getDay();
                    const daysInMonth = new Date(year, mIdx + 1, 0).getDate();
                    let dayCells = "";
                    for (let i = 0; i < firstDay; i++) dayCells += `<span></span>`;
                    for (let d = 1; d <= daysInMonth; d++) {
                        const dateStr = `${year}-${String(mIdx + 1).padStart(2, "0")}-${String(d).padStart(2, "0")}`;
                        const st = recMap.get(dateStr);
                        const cls = st === "Present" ? "bg-success-subtle text-success fw-bold"
                            : st === "Late" ? "bg-warning-subtle text-warning-emphasis fw-bold"
                            : st === "Absent" ? "bg-danger-subtle text-danger fw-bold"
                            : "text-muted";
                        dayCells += `<span class="d-inline-flex align-items-center justify-content-center rounded ${cls}" style="width:24px;height:24px;font-size:0.7rem;">${d}</span>`;
                    }
                    return `
                        <div class="col-md-6 col-xl-3">
                            <div class="border rounded p-2 bg-white h-100">
                                <div class="small fw-bold text-primary mb-2">${mName}</div>
                                <div style="display:grid;grid-template-columns:repeat(7,1fr);gap:2px;text-align:center;">
                                    <span class="text-muted" style="font-size:0.65rem;">S</span><span class="text-muted" style="font-size:0.65rem;">M</span><span class="text-muted" style="font-size:0.65rem;">T</span><span class="text-muted" style="font-size:0.65rem;">W</span><span class="text-muted" style="font-size:0.65rem;">T</span><span class="text-muted" style="font-size:0.65rem;">F</span><span class="text-muted" style="font-size:0.65rem;">S</span>
                                    ${dayCells}
                                </div>
                            </div>
                        </div>`;
                }).join("");
                $("#attendanceYearlyView").html(`<div class="row g-3">${cards}</div>`);
            }

            function refreshAttendanceViews() {
                const filtered = activeAttFilter
                    ? allAttendanceRecords.filter(r => r.status === activeAttFilter)
                    : allAttendanceRecords;
                renderAttendanceGrid(filtered);
                renderAttendanceYearly(filtered);
            }

            $(document).on("click", ".att-view-btn", function () {
                $(".att-view-btn").removeClass("active");
                $(this).addClass("active");
                const view = $(this).data("view");
                if (view === "yearly") {
                    $("#attendanceGridView").addClass("d-none");
                    $("#attendanceYearlyView").removeClass("d-none");
                } else {
                    $("#attendanceYearlyView").addClass("d-none");
                    $("#attendanceGridView").removeClass("d-none");
                }
            });

            $(document).on("click", ".att-filter-btn", function () {
                $(".att-filter-btn").removeClass("btn-dark").addClass("btn-outline-secondary");
                $(this).removeClass("btn-outline-secondary").addClass("btn-dark");
                activeAttFilter = $(this).data("status") || "";
                refreshAttendanceViews();
            });

            refreshAttendanceViews();
        })
        .fail(function () {
            $("#studentAttendanceBody").html('<tr><td colspan="6" class="text-center py-3 text-danger">Unable to load attendance records.</td></tr>');
        });

    $.get("/api/workspace/my-courses").done(function (courses) {
        if (!courses.length) {
            $("#studentWorkspaceCoursesRow").html('<div class="col-12 text-muted small">No courses enrolled yet.</div>');
            return;
        }
        const html = courses.map(c => `
            <div class="col-md-4">
                <div class="border rounded p-3 bg-light h-100 d-flex flex-column justify-content-between">
                    <div>
                        <span class="badge bg-info-subtle text-info-emphasis mb-1">${escapeHtml(c.courseCode || "COURSE")}</span>
                        <div class="fw-bold small">${escapeHtml(c.title)}</div>
                        <div class="text-muted" style="font-size:0.75rem;">${c.durationWeeks || 12} Weeks</div>
                    </div>
                    <a href="/Workspace/Course/${c.id}" class="btn btn-dark btn-sm mt-2">Open Course Workspace</a>
                </div>
            </div>`).join("");
        $("#studentWorkspaceCoursesRow").html(html);
    });

    function loadStudentActivities() {
        $.get("/api/workspace/student-activities")
            .done(function (res) {
                $("#stuOverviewQuizzes").text(res.quizzes || 0);
                $("#stuOverviewQuizzesCompleted").text(res.quizzesCompleted || 0);
                $("#stuOverviewAssignments").text(res.assignments || 0);
                $("#stuOverviewAssignmentsCompleted").text(res.assignmentsCompleted || 0);
                $("#stuOverviewForums").text(res.forums || 0);
                $("#stuOverviewForumsCompleted").text(res.forumsCompleted || 0);

                const items = res.items || [];
                const $tbody = $("#studentActivitiesTableBody");
                if ($tbody.length) {
                    if (!items.length) {
                        $tbody.html('<tr><td colspan="5" class="text-center py-3 text-muted small">No quizzes, assignments, or forum topics yet.</td></tr>');
                        return;
                    }
                    $tbody.html(items.map(a => `
                        <tr>
                            <td class="fw-semibold">${escapeHtml(a.title)}</td>
                            <td><span class="badge bg-light text-dark border">${escapeHtml(a.type)}</span></td>
                            <td class="small text-muted">${escapeHtml(a.courseTitle)}</td>
                            <td class="small">Week ${a.weekNumber}</td>
                            <td class="text-end">
                                <a href="/Workspace/Course/${a.courseId}" class="btn btn-sm btn-outline-primary">Open</a>
                            </td>
                        </tr>`).join(""));
                }
            });
    }

    loadCourseProgress();
    loadStudentActivities();
});
