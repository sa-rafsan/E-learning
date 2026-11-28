$(function () {
    let currentRoster = null;

    function escapeHtml(text) {
        return $("<div>").text(text ?? "").html();
    }

    function getInitials(name) {
        const parts = (name || "S").trim().split(/\s+/);
        return parts.length >= 2
            ? (parts[0][0] + parts[1][0]).toUpperCase()
            : parts[0].substring(0, 2).toUpperCase();
    }

    function showAlert(message, type) {
        $("#attendanceAlertContainer").html(`
            <div class="alert alert-${type} alert-dismissible fade show" role="alert">
                <span>${escapeHtml(message)}</span>
                <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
            </div>`);
    }

    function setTodayDate() {
        const today = new Date().toISOString().split("T")[0];
        $("#rosterDate").val(today);
    }

    function loadCourses() {
        $.get("/api/attendance/courses")
            .done(function (courses) {
                const options = courses.map(c =>
                    `<option value="${c.id}">${escapeHtml(c.title)}</option>`
                );
                $("#rosterCourse").html(options.join(""));
            })
            .fail(() => showAlert("Failed to load courses.", "danger"));
    }

    function updateSummaryCounts() {
        let total = 0, present = 0, absent = 0, late = 0, excused = 0;
        $("#rosterTableBody tr[data-student-id]").each(function () {
            total++;
            const val = parseInt($(this).attr("data-status"), 10) || 1;
            if (val === 1) present++;
            else if (val === 2) absent++;
            else if (val === 3) late++;
            else if (val === 4) excused++;
        });

        const rate = total > 0 ? Math.round(((present + late) / total) * 100) : 0;
        $("#countTotal").text(total);
        $("#countPresent").text(present);
        $("#countAbsent").text(absent);
        $("#countLate").text(late);
        $("#countExcused").text(excused);
        $("#attendanceRateBadge").text(`${rate}%`);
    }

    function statusStringToInt(statusStr) {
        switch (statusStr) {
            case "Absent": return 2;
            case "Late": return 3;
            case "Excused": return 4;
            default: return 1;
        }
    }

    function renderStatusButtons(selectedVal) {
        const statuses = [
            { val: 1, label: "Present", cls: "active-present" },
            { val: 3, label: "Late", cls: "active-late" },
            { val: 2, label: "Absent", cls: "active-absent" },
            { val: 4, label: "Excused", cls: "active-excused" }
        ];

        return statuses.map(s =>
            `<button type="button" class="att-pill-btn ${selectedVal === s.val ? s.cls : ""}" data-val="${s.val}" data-active-cls="${s.cls}">${s.label}</button>`
        ).join(" ");
    }

    function renderRoster(session) {
        currentRoster = session;
        const rows = session.records || [];
        $("#rosterTopic").val(session.topic || "");

        if (!rows.length) {
            $("#rosterTopicRow, #rosterSummaryBar, #saveRosterFooter").addClass("d-none");
            $("#rosterTableBody").html('<tr><td colspan="5" class="text-center py-4 text-muted">No students found for this batch and section.</td></tr>');
            return;
        }

        $("#rosterTopicRow, #rosterSummaryBar, #saveRosterFooter").removeClass("d-none");

        const html = rows.map((r, index) => {
            const statusVal = statusStringToInt(r.status);
            return `
                <tr data-student-id="${r.studentId}" data-status="${statusVal}">
                    <td class="text-muted">${index + 1}</td>
                    <td>
                        <div class="d-flex align-items-center gap-2">
                            <span class="dir-avatar" style="background-color:#dbeafe;color:#1d4ed8;">${escapeHtml(getInitials(r.fullName))}</span>
                            <div>
                                <div class="fw-semibold text-dark">${escapeHtml(r.fullName)}</div>
                                <div class="small text-muted">${escapeHtml(r.studentCode)}</div>
                            </div>
                        </div>
                    </td>
                    <td class="small text-muted">Batch ${escapeHtml(r.batch || "-")} - Sec ${escapeHtml(r.section || "-")}</td>
                    <td>
                        <div class="d-flex flex-wrap gap-1">${renderStatusButtons(statusVal)}</div>
                    </td>
                    <td>
                        <input type="text" class="form-control form-control-sm roster-remarks-input"
                               value="${escapeHtml(r.remarks || "")}" placeholder="Remarks (optional)..." />
                    </td>
                </tr>`;
        });

        $("#rosterTableBody").html(html.join(""));
        updateSummaryCounts();
    }

    function loadSessionsHistory() {
        $.get("/api/attendance/sessions")
            .done(function (sessions) {
                if (!sessions.length) {
                    $("#sessionsHistoryBody").html('<tr><td colspan="5" class="text-center py-3 text-muted">No attendance sessions recorded yet.</td></tr>');
                    return;
                }
                const html = sessions.map(s => `
                    <tr>
                        <td>${escapeHtml(s.sessionDate)}</td>
                        <td class="fw-semibold">${escapeHtml(s.courseTitle)}</td>
                        <td>Batch ${escapeHtml(s.batch)} - Section ${escapeHtml(s.section)}</td>
                        <td>${escapeHtml(s.topic || "-")}</td>
                        <td>
                            <span class="badge bg-success-subtle text-success">P: ${s.presentCount}</span>
                            <span class="badge bg-warning-subtle text-warning-emphasis">L: ${s.lateCount}</span>
                            <span class="badge bg-danger-subtle text-danger">A: ${s.absentCount}</span>
                            <span class="badge bg-info-subtle text-info-emphasis">E: ${s.excusedCount}</span>
                        </td>
                    </tr>`);
                $("#sessionsHistoryBody").html(html.join(""));
            });
    }

    $("[data-att-tab]").on("click", function () {
        const tab = $(this).data("att-tab");
        $("[data-att-tab]").removeClass("active");
        $(`[data-att-tab='${tab}']`).addClass("active");
        if (tab === "mark") {
            $("#tabSessionLogs").addClass("d-none");
            $("#tabMarkAttendance").removeClass("d-none");
        } else {
            $("#tabMarkAttendance").addClass("d-none");
            $("#tabSessionLogs").removeClass("d-none");
            loadSessionsHistory();
        }
    });

    $("#rosterFilterForm").on("submit", function (e) {
        e.preventDefault();
        const courseId = $("#rosterCourse").val();
        const sessionDate = $("#rosterDate").val();
        const batch = ($("#rosterBatch").val() || "").trim();
        const section = $("#rosterSection").val();

        if (!courseId || !sessionDate || !batch || !section) {
            showAlert("Please select course, date, batch, and section.", "warning");
            return;
        }

        const params = new URLSearchParams({ courseId, sessionDate, batch, section });
        $.get("/api/attendance/roster?" + params.toString())
            .done(renderRoster)
            .fail(xhr => showAlert(xhr.responseJSON?.message || "Failed to load roster.", "danger"));
    });

    $(document).on("click", ".att-pill-btn", function () {
        const $btn = $(this);
        const $row = $btn.closest("tr");
        const val = parseInt($btn.data("val"), 10);
        const activeCls = $btn.data("active-cls");

        $row.attr("data-status", val);
        $row.find(".att-pill-btn").removeClass("active-present active-late active-absent active-excused");
        $btn.addClass(activeCls);
        updateSummaryCounts();
    });

    $("#markAllPresentBtn").on("click", function () {
        $("#rosterTableBody tr[data-student-id]").each(function () {
            $(this).attr("data-status", 1);
            $(this).find(".att-pill-btn").removeClass("active-present active-late active-absent active-excused");
            $(this).find(".att-pill-btn[data-val='1']").addClass("active-present");
        });
        updateSummaryCounts();
    });

    $("#markAllAbsentBtn").on("click", function () {
        $("#rosterTableBody tr[data-student-id]").each(function () {
            $(this).attr("data-status", 2);
            $(this).find(".att-pill-btn").removeClass("active-present active-late active-absent active-excused");
            $(this).find(".att-pill-btn[data-val='2']").addClass("active-absent");
        });
        updateSummaryCounts();
    });

    $("#saveAttendanceBtn").on("click", function () {
        if (!currentRoster) return;

        const records = [];
        $("#rosterTableBody tr[data-student-id]").each(function () {
            const $row = $(this);
            records.push({
                studentId: parseInt($row.data("student-id"), 10),
                status: parseInt($row.attr("data-status"), 10) || 1,
                remarks: ($row.find(".roster-remarks-input").val() || "").trim() || null
            });
        });

        $.ajax({
            url: "/api/attendance/roster",
            method: "POST",
            contentType: "application/json",
            data: JSON.stringify({
                courseId: currentRoster.courseId,
                sessionDate: currentRoster.sessionDate,
                batch: currentRoster.batch,
                section: currentRoster.section,
                topic: ($("#rosterTopic").val() || "").trim() || null,
                records
            }),
            success: function (saved) {
                renderRoster(saved);
                loadSessionsHistory();
                showAlert("Attendance session saved successfully.", "success");
            },
            error: function (xhr) {
                showAlert(xhr.responseJSON?.message || "Failed to save attendance.", "danger");
            }
        });
    });

    function loadAssignedCourses() {
        $.get("/api/workspace/my-courses").done(function (courses) {
            if (!courses.length) {
                $("#teacherAssignedCoursesRow").html('<div class="col-12 text-muted small">No courses assigned yet.</div>');
                return;
            }
            const html = courses.map(c => `
                <div class="col-md-4">
                    <div class="border rounded p-3 bg-light h-100 d-flex flex-column justify-content-between">
                        <div>
                            <span class="badge bg-primary-subtle text-primary mb-1">${escapeHtml(c.courseCode || "COURSE")}</span>
                            <div class="fw-bold small">${escapeHtml(c.title)}</div>
                            <div class="text-muted" style="font-size:0.75rem;">${c.durationWeeks || 12} Weeks &bull; ${(c.batches || []).length} Batches</div>
                        </div>
                        <a href="/Workspace/Course/${c.id}" class="btn btn-primary btn-sm mt-2">Open Course Workspace</a>
                    </div>
                </div>`).join("");
            $("#teacherAssignedCoursesRow").html(html);
        });
    }

    setTodayDate();
    loadCourses();
    loadAssignedCourses();
    loadSessionsHistory();
});
