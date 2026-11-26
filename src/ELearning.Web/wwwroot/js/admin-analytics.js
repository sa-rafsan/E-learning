$(function () {
    function buildDonutSvg(segments, centerTop, centerBottom) {
        const total = Math.max(1, segments.reduce((acc, s) => acc + s.value, 0));
        const r = 46;
        const c = 2 * Math.PI * r;
        let offset = 0;

        const circles = segments.map(seg => {
            const len = (seg.value / total) * c;
            const el = `<circle cx="64" cy="64" r="${r}" fill="none" stroke="${seg.color}" stroke-width="18"
                stroke-dasharray="${len} ${c}" stroke-dashoffset="${-offset}" transform="rotate(-90 64 64)" />`;
            offset += len;
            return el;
        }).join("");

        return `
            <svg width="128" height="128" viewBox="0 0 128 128">
                <circle cx="64" cy="64" r="${r}" fill="none" stroke="#e2e8f0" stroke-width="18" />
                ${circles}
                <text x="64" y="61" text-anchor="middle" font-size="18" font-weight="700" fill="#0f172a">${centerTop}</text>
                <text x="64" y="76" text-anchor="middle" font-size="9" font-weight="700" fill="#64748b">${centerBottom}</text>
            </svg>`;
    }

    function renderAttendanceChart(att) {
        const total = att.present + att.absent + att.late + att.excused;
        const pct = total > 0 ? Math.round((att.present / total) * 100) : 0;

        $("#chartAttendanceDonut").html(buildDonutSvg([
            { value: att.present, color: "#10b981" },
            { value: att.absent, color: "#ef4444" },
            { value: att.late, color: "#f59e0b" },
            { value: att.excused, color: "#8b5cf6" }
        ], `${pct}%`, "PRESENT"));

        const pctOf = v => total > 0 ? Math.round((v / total) * 100) : 0;
        $("#chartAttendanceLegend").html(`
            <div class="att-legend-pill" style="background:#ecfdf5;border-color:#a7f3d0;">
                <span><span style="color:#10b981;">&#9679;</span> Present</span>
                <span class="text-success">${pctOf(att.present)}% (${att.present})</span>
            </div>
            <div class="att-legend-pill" style="background:#fef2f2;border-color:#fecaca;">
                <span><span style="color:#ef4444;">&#9679;</span> Absent</span>
                <span class="text-danger">${pctOf(att.absent)}% (${att.absent})</span>
            </div>
            <div class="att-legend-pill" style="background:#fffbeb;border-color:#fde68a;">
                <span><span style="color:#f59e0b;">&#9679;</span> Late</span>
                <span class="text-warning-emphasis">${pctOf(att.late)}% (${att.late})</span>
            </div>
            <div class="att-legend-pill mb-0" style="background:#f5f3ff;border-color:#ddd6fe;">
                <span><span style="color:#8b5cf6;">&#9679;</span> Excused</span>
                <span style="color:#7c3aed;">${pctOf(att.excused)}% (${att.excused})</span>
            </div>
        `);
    }

    function renderSectionBars(sections) {
        const maxVal = Math.max(1, ...sections.flatMap(s => [s.active, s.inactive]));
        const html = sections.map(s => {
            const actH = Math.max(4, Math.round((s.active / maxVal) * 125));
            const inactH = s.inactive > 0 ? Math.max(4, Math.round((s.inactive / maxVal) * 125)) : 2;
            return `
                <div class="text-center">
                    <div class="grouped-bar-pair">
                        <div class="grouped-bar-green" style="height:${actH}px;" title="Active: ${s.active}"></div>
                        <div class="grouped-bar-red" style="height:${inactH}px;" title="Inactive: ${s.inactive}"></div>
                    </div>
                    <div class="small text-muted mt-1">${s.section}</div>
                </div>`;
        }).join("");
        $("#chartSectionGroupedBars").html(html);
    }

    function renderRoleBreakdown(roles) {
        const total = roles.students + roles.teachers + roles.admins;
        $("#chartRoleDonut").html(buildDonutSvg([
            { value: roles.students, color: "#f59e0b" },
            { value: roles.teachers, color: "#f97316" },
            { value: roles.admins, color: "#e11d48" }
        ], `${total}`, "USERS"));

        $("#chartRoleBreakdown").html(`
            <div class="d-flex justify-content-between py-1 border-bottom small">
                <span class="text-muted">Students</span><strong>${roles.students}</strong>
            </div>
            <div class="d-flex justify-content-between py-1 border-bottom small">
                <span class="text-muted">Teachers</span><strong>${roles.teachers}</strong>
            </div>
            <div class="d-flex justify-content-between py-1 border-bottom small">
                <span class="text-muted">Active Accounts</span><strong class="text-success">${roles.active}</strong>
            </div>
            <div class="d-flex justify-content-between py-1 small">
                <span class="text-muted">Inactive Accounts</span><strong class="text-danger">${roles.inactive}</strong>
            </div>
        `);
    }

    function renderPipeline(p) {
        $("#chartAcademicPipeline").html(`
            <div class="col-6">
                <div class="pipeline-tile pipeline-blue">
                    <div class="small fw-bold text-uppercase">Published Courses</div>
                    <div class="h3 fw-bold my-1">${p.publishedCourses}</div>
                    <div class="small">Of ${p.totalCourses} total</div>
                </div>
            </div>
            <div class="col-6">
                <div class="pipeline-tile pipeline-purple">
                    <div class="small fw-bold text-uppercase">Assigned Courses</div>
                    <div class="h3 fw-bold my-1">${p.assignedCourses}</div>
                    <div class="small">With Faculty</div>
                </div>
            </div>
            <div class="col-6">
                <div class="pipeline-tile pipeline-amber">
                    <div class="small fw-bold text-uppercase">Enrollments</div>
                    <div class="h3 fw-bold my-1">${p.totalEnrollments}</div>
                    <div class="small">Course Seats</div>
                </div>
            </div>
            <div class="col-6">
                <div class="pipeline-tile pipeline-green">
                    <div class="small fw-bold text-uppercase">Sessions Logged</div>
                    <div class="h3 fw-bold my-1">${p.totalSessions}</div>
                    <div class="small">Attendance Rosters</div>
                </div>
            </div>
        `);
    }

    function renderAreaChart(series) {
        if (!series || !series.length) {
            $("#chartCourseAreaTrend").html('<div class="text-muted small py-5 text-center">No course data yet.</div>');
            return;
        }

        const w = 480, h = 165, padL = 32, padR = 16, padT = 16, padB = 28;
        const innerW = w - padL - padR;
        const innerH = h - padT - padB;
        const values = series.map(s => Math.max(s.enrolled, s.hours));
        const maxV = Math.max(5, ...values);

        const pts = series.map((s, idx) => {
            const x = series.length === 1 ? padL + innerW / 2 : padL + (idx / (series.length - 1)) * innerW;
            const y = padT + innerH - ((Math.max(s.enrolled, s.hours) / maxV) * innerH);
            return { x, y, label: s.label };
        });

        const linePath = pts.map((p, i) => `${i === 0 ? "M" : "L"}${p.x.toFixed(1)},${p.y.toFixed(1)}`).join(" ");
        const areaPath = `${linePath} L${pts[pts.length - 1].x.toFixed(1)},${padT + innerH} L${pts[0].x.toFixed(1)},${padT + innerH} Z`;
        const labels = pts.map(p => `<text x="${p.x.toFixed(1)}" y="${h - 6}" text-anchor="middle" font-size="9" fill="#64748b">${p.label}</text>`).join("");

        $("#chartCourseAreaTrend").html(`
            <svg viewBox="0 0 ${w} ${h}" class="w-100" style="height:175px;">
                <defs>
                    <linearGradient id="courseAreaGrad" x1="0" y1="0" x2="0" y2="1">
                        <stop offset="0%" stop-color="#2563eb" stop-opacity="0.32" />
                        <stop offset="100%" stop-color="#2563eb" stop-opacity="0.02" />
                    </linearGradient>
                </defs>
                <line x1="${padL}" y1="${padT}" x2="${padL}" y2="${padT + innerH}" stroke="#cbd5e1" />
                <line x1="${padL}" y1="${padT + innerH}" x2="${w - padR}" y2="${padT + innerH}" stroke="#cbd5e1" />
                <path d="${areaPath}" fill="url(#courseAreaGrad)" />
                <path d="${linePath}" fill="none" stroke="#2563eb" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" />
                ${labels}
            </svg>
        `);
    }

    function loadAnalytics() {
        $.get("/api/admin/analytics").done(function (data) {
            renderAttendanceChart(data.attendance);
            renderSectionBars(data.sections);
            renderRoleBreakdown(data.roles);
            renderPipeline(data.pipeline);
            renderAreaChart(data.courseSeries);
        });
    }

    loadAnalytics();
});

