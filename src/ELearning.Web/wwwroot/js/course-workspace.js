$(function () {
    const $root = $("#workspaceRoot");
    const courseId = $root.data("course-id");
    const canManage = $root.data("can-manage") === true || $root.data("can-manage") === "true";
    const isStudent = $root.data("is-student") === true || $root.data("is-student") === "true";

    let workspaceData = null;
    let quizTimerInterval = null;

    const activityModal = new bootstrap.Modal(document.getElementById("activityDetailModal"));
    const editWeekModal = canManage ? new bootstrap.Modal(document.getElementById("editWeekModal")) : null;
    const addTopicModal = canManage ? new bootstrap.Modal(document.getElementById("addTopicModal")) : null;
    const addMaterialModal = canManage ? new bootstrap.Modal(document.getElementById("addMaterialModal")) : null;
    const createActivityModal = canManage ? new bootstrap.Modal(document.getElementById("createActivityModal")) : null;
    const submissionsModal = canManage ? new bootstrap.Modal(document.getElementById("submissionsModal")) : null;

    function esc(t) { return $("<div>").text(t ?? "").html(); }

    function showToast(title, message, isError) {
        $("#adminToastContainer").html(`
            <div class="admin-toast-card ${isError ? "" : "toast-success"}" role="alert">
                <div class="fw-bold ${isError ? "text-danger" : "text-success"}">${isError ? "&#9888;" : "&#10003;"}</div>
                <div class="flex-grow-1"><div class="fw-bold small">${esc(title)}</div><div class="small text-muted">${esc(message)}</div></div>
                <button type="button" class="btn-close btn-sm" onclick="$('#adminToastContainer').empty()"></button>
            </div>`);
        setTimeout(() => $("#adminToastContainer").empty(), 4500);
    }

    $("[data-ws-tab]").on("click", function () {
        const tab = $(this).data("ws-tab");
        $(".admin-nav-link[data-ws-tab]").removeClass("active");
        $(`.admin-nav-link[data-ws-tab='${tab}']`).addClass("active");
        if (tab === "general") {
            $("#wsWeeksTab").addClass("d-none");
            $("#wsGeneralTab").removeClass("d-none");
        } else {
            $("#wsGeneralTab").addClass("d-none");
            $("#wsWeeksTab").removeClass("d-none");
        }
    });

    function loadWorkspace() {
        $.get(`/api/workspace/courses/${courseId}`).done(function (data) {
            workspaceData = data;
            const g = data.general;
            $("#wsCourseTitle").text(`${g.title} (${g.courseCode})`);
            $("#wsCourseSubtitle").text(`${g.category} • ${g.durationWeeks} Weeks • ${g.status}`);

            renderGeneral(g);
            renderWeeks(data.weeks);

            if (canManage) {
                const batchOpts = (g.batches || []).map(b => `<option value="${b.id}">${esc(b.batchName)} (${esc(b.batchCode)})</option>`).join("");
                $("#actBatchId").html(`<option value="">All Batches</option>${batchOpts}`);
            }
        }).fail(() => showToast("Error", "Unable to load course workspace.", true));
    }

    function renderGeneral(g) {
        const img = g.imagePath
            ? `<img src="${esc(g.imagePath)}" class="rounded object-fit-cover border" style="width:140px;height:96px;" alt="Course" />`
            : "";

        $("#wsGeneralCourseCard").html(`
            <div class="col-12">
                <div class="user-table-card p-4 d-flex flex-wrap gap-3 align-items-center">
                    ${img}
                    <div class="flex-grow-1">
                        <div class="d-flex flex-wrap gap-2 mb-1">
                            <span class="badge bg-primary-subtle text-primary">${esc(g.courseCode)}</span>
                            <span class="badge bg-secondary-subtle text-secondary">${esc(g.category)}</span>
                            <span class="badge bg-info-subtle text-info-emphasis">${g.durationWeeks} Weeks</span>
                        </div>
                        <h2 class="h5 fw-bold mb-1">${esc(g.title)}</h2>
                        <p class="text-muted small mb-0">${esc(g.description)}</p>
                    </div>
                </div>
            </div>`);

        const tHtml = (g.teachers || []).length
            ? g.teachers.map(t => {
                const quals = (t.qualifications || []).map(q => `<li>${esc(q.degreeName)} — ${esc(q.institutionName)} (${q.passingYear || "-"})</li>`).join("");
                const photo = t.imagePath
                    ? `<img src="${esc(t.imagePath)}" class="rounded-circle object-fit-cover border" style="width:52px;height:52px;" />`
                    : `<span class="dir-avatar" style="width:52px;height:52px;background:#f3e8ff;color:#7e22ce;">${esc((t.fullName || "T").substring(0, 2).toUpperCase())}</span>`;
                return `
                    <div class="col-md-6">
                        <div class="user-table-card p-3 h-100">
                            <div class="d-flex align-items-center gap-3 mb-2">
                                ${photo}
                                <div>
                                    <div class="fw-bold">${esc(t.fullName)}</div>
                                    <div class="small text-muted">${esc(t.email)} &bull; ${esc(t.phoneNumber || "-")}</div>
                                </div>
                            </div>
                            <div class="small text-muted mb-1"><strong>Specialization:</strong> ${esc(t.specialization || "-")}</div>
                            <div class="small text-muted mb-1"><strong>Experience:</strong> ${esc(t.professionalExperience || "-")}</div>
                            <div class="small text-muted mb-1"><strong>Location:</strong> ${esc(t.address || "-")} ${t.country ? `(${esc(t.country)})` : ""}</div>
                            ${quals ? `<div class="small mt-2"><strong>Qualifications:</strong><ul class="mb-0 ps-3">${quals}</ul></div>` : ""}
                        </div>
                    </div>`;
            }).join("")
            : '<div class="col-12 text-muted small">No teachers assigned yet.</div>';
        $("#wsGeneralTeachers").html(tHtml);

        const bHtml = (g.batches || []).length
            ? g.batches.map(b => {
                const tNames = (b.teachers || []).map(t => esc(t.fullName)).join(", ") || "TBA";
                return `
                    <div class="col-md-6">
                        <div class="user-table-card p-3 h-100">
                            <div class="d-flex justify-content-between align-items-center mb-1">
                                <strong>${esc(b.batchName)} (${esc(b.batchCode)})</strong>
                                <span class="badge bg-success-subtle text-success">${esc(b.status)}</span>
                            </div>
                            <div class="small text-muted mb-1"><strong>Teachers:</strong> ${tNames}</div>
                            <div class="small text-muted mb-1"><strong>Duration:</strong> ${b.startDate} to ${b.endDate}</div>
                            <div class="small text-muted mb-1"><strong>Schedule:</strong> ${esc(b.schedule || "-")}</div>
                            <div class="small text-muted"><strong>Enrolled Students:</strong> ${(b.enrolledStudents || []).length}</div>
                        </div>
                    </div>`;
            }).join("")
            : '<div class="col-12 text-muted small">No batches created yet.</div>';
        $("#wsGeneralBatches").html(bHtml);
    }

    function renderWeeks(weeks) {
        const html = weeks.map(w => {
            const topicsHtml = (w.topics || []).length
                ? w.topics.map(t => `
                    <li class="d-flex justify-content-between align-items-center py-1 border-bottom small">
                        <span><strong>${t.order}. ${esc(t.title)}</strong> ${t.description ? `<span class="text-muted">— ${esc(t.description)}</span>` : ""}</span>
                        ${canManage ? `<button type="button" class="btn btn-sm btn-link text-danger p-0 btn-del-topic" data-id="${t.id}">Remove</button>` : ""}
                    </li>`).join("")
                : '<li class="text-muted small">No topics added yet.</li>';

            const matsHtml = (w.materials || []).length
                ? w.materials.map(m => {
                    const href = m.filePath || m.externalUrl || "#";
                    return `<a href="${esc(href)}" target="_blank" class="badge bg-light text-primary border text-decoration-none me-1 mb-1 p-2">${esc(m.title)} (${esc(m.materialType)})</a>`;
                }).join("")
                : '<span class="text-muted small">No learning materials yet.</span>';

            const actsHtml = (w.activities || []).length
                ? w.activities.map(a => {
                    const subBadge = a.mySubmission
                        ? `<span class="badge bg-success-subtle text-success ms-1">${a.mySubmission.isEvaluated ? `Marks: ${a.mySubmission.obtainedMarks}/${a.maxMarks}` : "Submitted"}</span>`
                        : "";
                    const lockText = !a.isUnlocked ? `<span class="badge bg-secondary-subtle text-secondary">Opens ${new Date(a.startAtUtc).toLocaleString()}</span>` : "";
                    return `
                        <div class="d-flex flex-wrap justify-content-between align-items-center gap-2 p-2 border rounded bg-light mb-2">
                            <div>
                                <span class="badge bg-primary-subtle text-primary me-1">${esc(a.type)}</span>
                                <strong class="small">${esc(a.title)}</strong> ${subBadge} ${lockText}
                                <div class="text-muted" style="font-size:0.75rem;">
                                    Start: ${new Date(a.startAtUtc).toLocaleString()} &bull; Deadline: ${new Date(a.submissionDeadlineUtc || a.endAtUtc).toLocaleString()} &bull; Max: ${a.maxMarks}
                                </div>
                            </div>
                            <div class="d-flex gap-1">
                                <button type="button" class="btn btn-sm btn-outline-primary btn-open-activity" data-id="${a.id}" ${!a.isUnlocked && isStudent ? "disabled" : ""}>
                                    ${isStudent ? "Open" : "Preview"}
                                </button>
                                ${canManage ? `<button type="button" class="btn btn-sm btn-outline-dark btn-view-subs" data-id="${a.id}">Submissions (${a.submissionsCount})</button>` : ""}
                            </div>
                        </div>`;
                }).join("")
                : '<div class="text-muted small">No activities scheduled for this week.</div>';

            return `
                <div class="user-table-card p-3 p-md-4">
                    <div class="d-flex flex-wrap justify-content-between align-items-start gap-2 pb-2 mb-3 border-bottom">
                        <div>
                            <span class="badge bg-dark text-white mb-1">Week ${String(w.weekNumber).padStart(2, "0")}</span>
                            <h3 class="h6 fw-bold mb-0">${esc(w.title)}</h3>
                            <div class="small text-muted">${esc(w.description || "")}</div>
                            ${w.learningObjectives ? `<div class="small text-primary mt-1"><strong>Objectives:</strong> ${esc(w.learningObjectives)}</div>` : ""}
                        </div>
                        ${canManage ? `
                            <div class="d-flex flex-wrap gap-1">
                                <button type="button" class="btn btn-sm btn-outline-secondary btn-edit-week" data-week-id="${w.id}">Edit Week</button>
                                <button type="button" class="btn btn-sm btn-outline-primary btn-add-topic" data-week-id="${w.id}">+ Topic</button>
                                <button type="button" class="btn btn-sm btn-outline-info btn-add-mat" data-week-id="${w.id}">+ Material</button>
                                <button type="button" class="btn btn-sm btn-primary btn-add-act" data-week-id="${w.id}">+ Activity</button>
                            </div>` : ""}
                    </div>
                    <div class="row g-3">
                        <div class="col-md-6">
                            <div class="small fw-bold text-uppercase text-muted mb-1">Topics</div>
                            <ul class="list-unstyled mb-3">${topicsHtml}</ul>
                            <div class="small fw-bold text-uppercase text-muted mb-1">Learning Materials</div>
                            <div>${matsHtml}</div>
                        </div>
                        <div class="col-md-6">
                            <div class="small fw-bold text-uppercase text-muted mb-1">Weekly Activities &amp; Assessments</div>
                            ${actsHtml}
                        </div>
                    </div>
                </div>`;
        }).join("");
        $("#wsWeeksContainer").html(html);
    }

    if (canManage) {
        $(document).on("click", ".btn-edit-week", function () {
            const wId = $(this).data("week-id");
            const w = workspaceData.weeks.find(x => x.id === wId);
            if (!w) return;
            $("#editWeekId").val(w.id);
            $("#editWeekTitle").val(w.title);
            $("#editWeekDesc").val(w.description || "");
            $("#editWeekObj").val(w.learningObjectives || "");
            $("#editWeekStart").val(w.startDate || "");
            $("#editWeekEnd").val(w.endDate || "");
            editWeekModal.show();
        });

        $("#editWeekForm").on("submit", function (e) {
            e.preventDefault();
            const wId = $("#editWeekId").val();
            $.ajax({
                url: `/api/workspace/weeks/${wId}`,
                method: "PUT",
                contentType: "application/json",
                data: JSON.stringify({
                    title: $("#editWeekTitle").val(),
                    description: $("#editWeekDesc").val(),
                    learningObjectives: $("#editWeekObj").val(),
                    startDate: $("#editWeekStart").val() || null,
                    endDate: $("#editWeekEnd").val() || null
                }),
                success: () => { editWeekModal.hide(); showToast("Updated", "Week updated.", false); loadWorkspace(); }
            });
        });

        $(document).on("click", ".btn-add-topic", function () {
            $("#addTopicForm")[0].reset();
            $("#topicWeekId").val($(this).data("week-id"));
            addTopicModal.show();
        });

        $("#addTopicForm").on("submit", function (e) {
            e.preventDefault();
            const wId = $("#topicWeekId").val();
            $.ajax({
                url: `/api/workspace/weeks/${wId}/topics`,
                method: "POST",
                contentType: "application/json",
                data: JSON.stringify({
                    title: $("#topicTitle").val(),
                    description: $("#topicDesc").val(),
                    order: parseInt($("#topicOrder").val(), 10) || 1
                }),
                success: () => { addTopicModal.hide(); showToast("Added", "Topic added.", false); loadWorkspace(); }
            });
        });

        $(document).on("click", ".btn-del-topic", function () {
            $.ajax({ url: `/api/workspace/topics/${$(this).data("id")}`, method: "DELETE", success: loadWorkspace });
        });

        $(document).on("click", ".btn-add-mat", function () {
            $("#addMaterialForm")[0].reset();
            $("#matWeekId").val($(this).data("week-id"));
            $("#matFilePath").val("");
            addMaterialModal.show();
        });

        $("#matFileInput").on("change", function () {
            if (!this.files[0]) return;
            const fd = new FormData();
            fd.append("file", this.files[0]);
            $.ajax({
                url: "/api/workspace/upload",
                method: "POST",
                data: fd,
                processData: false,
                contentType: false,
                success: res => $("#matFilePath").val(res.filePath)
            });
        });

        $("#addMaterialForm").on("submit", function (e) {
            e.preventDefault();
            const wId = $("#matWeekId").val();
            $.ajax({
                url: `/api/workspace/weeks/${wId}/materials`,
                method: "POST",
                contentType: "application/json",
                data: JSON.stringify({
                    title: $("#matTitle").val(),
                    materialType: $("#matType").val(),
                    filePath: $("#matFilePath").val() || null,
                    externalUrl: $("#matUrl").val() || null
                }),
                success: () => { addMaterialModal.hide(); showToast("Added", "Material added.", false); loadWorkspace(); }
            });
        });

        $(document).on("click", ".btn-add-act", function () {
            $("#createActivityForm")[0].reset();
            $("#actWeekId").val($(this).data("week-id"));
            $("#actAttachmentPath").val("");
            $("#actQuestionsContainer").empty();
            const now = new Date();
            const later = new Date(now.getTime() + 7 * 86400000);
            $("#actStart").val(now.toISOString().slice(0, 16));
            $("#actEnd").val(later.toISOString().slice(0, 16));
            $("#actQuestionsSection").addClass("d-none");
            createActivityModal.show();
        });

        $("#actType").on("change", function () {
            const v = $(this).val();
            if (v === "3" || v === "5") $("#actQuestionsSection").removeClass("d-none");
            else $("#actQuestionsSection").addClass("d-none");
        });

        $("#addActQuestionBtn").on("click", function () {
            $("#actQuestionsContainer").append(`
                <div class="border rounded p-2 bg-light act-q-row">
                    <div class="row g-1 mb-1">
                        <div class="col-md-7"><input type="text" class="form-control form-control-sm q-text" placeholder="Question text..." required /></div>
                        <div class="col-md-3">
                            <select class="form-select form-select-sm q-type">
                                <option value="1">MCQ</option><option value="2">True/False</option><option value="3">Short Answer</option>
                            </select>
                        </div>
                        <div class="col-md-2"><input type="number" class="form-control form-control-sm q-marks" value="5" placeholder="Marks" /></div>
                    </div>
                    <div class="row g-1">
                        <div class="col-md-2"><input type="text" class="form-control form-control-sm q-a" placeholder="Option A" /></div>
                        <div class="col-md-2"><input type="text" class="form-control form-control-sm q-b" placeholder="Option B" /></div>
                        <div class="col-md-2"><input type="text" class="form-control form-control-sm q-c" placeholder="Option C" /></div>
                        <div class="col-md-2"><input type="text" class="form-control form-control-sm q-d" placeholder="Option D" /></div>
                        <div class="col-md-4"><input type="text" class="form-control form-control-sm q-ans" placeholder="Correct Answer (e.g. Option A)" /></div>
                    </div>
                </div>`);
        });

        $("#actFileInput").on("change", function () {
            if (!this.files[0]) return;
            const fd = new FormData();
            fd.append("file", this.files[0]);
            $.ajax({
                url: "/api/workspace/upload",
                method: "POST",
                data: fd,
                processData: false,
                contentType: false,
                success: res => $("#actAttachmentPath").val(res.filePath)
            });
        });

        $("#createActivityForm").on("submit", function (e) {
            e.preventDefault();
            const wId = $("#actWeekId").val();
            const questions = [];
            $(".act-q-row").each(function () {
                const qt = ($(this).find(".q-text").val() || "").trim();
                if (!qt) return;
                questions.push({
                    questionText: qt,
                    type: parseInt($(this).find(".q-type").val(), 10),
                    marks: parseFloat($(this).find(".q-marks").val()) || 1,
                    optionA: $(this).find(".q-a").val() || null,
                    optionB: $(this).find(".q-b").val() || null,
                    optionC: $(this).find(".q-c").val() || null,
                    optionD: $(this).find(".q-d").val() || null,
                    correctOption: $(this).find(".q-ans").val() || null
                });
            });

            $.ajax({
                url: `/api/workspace/weeks/${wId}/activities`,
                method: "POST",
                contentType: "application/json",
                data: JSON.stringify({
                    courseBatchId: $("#actBatchId").val() ? parseInt($("#actBatchId").val(), 10) : null,
                    type: parseInt($("#actType").val(), 10),
                    status: parseInt($("#actStatus").val(), 10),
                    title: $("#actTitle").val(),
                    instructions: $("#actInstructions").val(),
                    startAtUtc: new Date($("#actStart").val()).toISOString(),
                    endAtUtc: new Date($("#actEnd").val()).toISOString(),
                    maxMarks: parseFloat($("#actMaxMarks").val()) || 100,
                    timeLimitMinutes: $("#actTimeLimit").val() ? parseInt($("#actTimeLimit").val(), 10) : null,
                    allowLateSubmission: $("#actAllowLate").is(":checked"),
                    attachmentPath: $("#actAttachmentPath").val() || null,
                    questions
                }),
                success: () => { createActivityModal.hide(); showToast("Created", "Activity scheduled.", false); loadWorkspace(); },
                error: xhr => showToast("Error", xhr.responseJSON?.message || "Unable to create activity.", true)
            });
        });

        $(document).on("click", ".btn-view-subs", function () {
            const aId = $(this).data("id");
            $.get(`/api/workspace/activities/${aId}/submissions`).done(function (subs) {
                if (!subs.length) {
                    $("#submissionsModalBody").html('<div class="text-muted small">No submissions yet.</div>');
                    submissionsModal.show();
                    return;
                }
                const html = subs.map(s => `
                    <div class="border rounded p-3 mb-2">
                        <div class="d-flex justify-content-between">
                            <strong>${esc(s.studentName)} (${esc(s.studentCode)})</strong>
                            <span class="small text-muted">${new Date(s.submittedAtUtc).toLocaleString()}</span>
                        </div>
                        <div class="small my-2">${esc(s.submissionText || "")}</div>
                        ${s.attachmentPath ? `<a href="${esc(s.attachmentPath)}" target="_blank" class="btn btn-sm btn-outline-primary mb-2">Download Attachment</a>` : ""}
                        <div class="row g-2 align-items-end">
                            <div class="col-md-3"><label class="small">Marks</label><input type="number" class="form-control form-control-sm eval-marks" value="${s.obtainedMarks ?? ""}" /></div>
                            <div class="col-md-6"><label class="small">Feedback</label><input type="text" class="form-control form-control-sm eval-fb" value="${esc(s.feedback || "")}" /></div>
                            <div class="col-md-3"><button type="button" class="btn btn-sm btn-dark w-100 btn-save-eval" data-sub-id="${s.id}">Save &amp; Publish</button></div>
                        </div>
                    </div>`).join("");
                $("#submissionsModalBody").html(html);
                submissionsModal.show();
            });
        });

        $(document).on("click", ".btn-save-eval", function () {
            const $card = $(this).closest(".border");
            const subId = $(this).data("sub-id");
            $.ajax({
                url: `/api/workspace/submissions/${subId}/evaluate`,
                method: "POST",
                contentType: "application/json",
                data: JSON.stringify({
                    obtainedMarks: parseFloat($card.find(".eval-marks").val()) || 0,
                    feedback: $card.find(".eval-fb").val(),
                    isPublished: true
                }),
                success: () => { showToast("Evaluated", "Marks & feedback published.", false); loadWorkspace(); }
            });
        });
    }

    $(document).on("click", ".btn-open-activity", function () {
        clearInterval(quizTimerInterval);
        const aId = $(this).data("id");
        $.get(`/api/workspace/activities/${aId}`).done(function (a) {
            $("#actDetailTitle").text(`${a.type}: ${a.title}`);
            let body = `<p class="small text-muted">${esc(a.instructions || "")}</p>`;
            if (a.attachmentPath) {
                body += `<p><a href="${esc(a.attachmentPath)}" target="_blank" class="btn btn-sm btn-outline-primary">Download Attachment</a></p>`;
            }

            if (a.type === "Forum") {
                const posts = (a.forumPosts || []).map(p => `
                    <div class="border rounded p-2 mb-2 bg-light">
                        <div class="small fw-bold">${esc(p.authorName)} <span class="badge bg-secondary-subtle text-secondary">${esc(p.authorRole)}</span></div>
                        <div class="small">${esc(p.content)}</div>
                    </div>`).join("");
                body += `<div id="forumPostsList">${posts || '<div class="text-muted small mb-2">No discussions yet.</div>'}</div>
                    <form id="forumReplyForm" data-act-id="${a.id}" class="mt-3">
                        <textarea class="form-control mb-2" id="forumContent" rows="2" placeholder="Write a comment..." required></textarea>
                        <button type="submit" class="btn btn-primary btn-sm">Post Comment</button>
                    </form>`;
            } else if (isStudent) {
                if (a.mySubmission) {
                    body += `<div class="alert alert-success small">
                        Submitted at ${new Date(a.mySubmission.submittedAtUtc).toLocaleString()}.
                        ${a.mySubmission.isEvaluated ? `<strong>Marks: ${a.mySubmission.obtainedMarks}/${a.maxMarks}</strong>` : "Pending evaluation."}
                        ${a.mySubmission.feedback ? `<div>Feedback: ${esc(a.mySubmission.feedback)}</div>` : ""}
                    </div>`;
                }

                if (a.timeLimitMinutes && !a.mySubmission) {
                    body += `<div class="alert alert-warning small fw-bold" id="quizTimerBanner">Time Remaining: ${a.timeLimitMinutes}:00</div>`;
                }

                let qHtml = "";
                if (a.questions && a.questions.length) {
                    qHtml = a.questions.map((q, idx) => `
                        <div class="border rounded p-2 mb-2 q-answer-box" data-q-id="${q.id}">
                            <div class="small fw-semibold mb-1">${idx + 1}. ${esc(q.questionText)} (${q.marks} pts)</div>
                            ${q.type === "ShortAnswer"
                                ? `<input type="text" class="form-control form-control-sm q-ans-input" placeholder="Your answer..." />`
                                : [q.optionA, q.optionB, q.optionC, q.optionD].filter(Boolean).map(opt => `
                                    <div class="form-check small">
                                        <input class="form-check-input q-ans-radio" type="radio" name="q_${q.id}" value="${esc(opt)}" />
                                        <label class="form-check-label">${esc(opt)}</label>
                                    </div>`).join("")}
                        </div>`).join("");
                }

                body += `<form id="studentSubmitActivityForm" data-act-id="${a.id}">
                    ${qHtml}
                    ${(!a.questions || !a.questions.length) ? `
                        <div class="mb-2"><label class="small fw-semibold">Submission Text</label><textarea class="form-control" id="subText" rows="3"></textarea></div>
                        <div class="mb-2"><label class="small fw-semibold">Upload File (Optional)</label><input type="file" class="form-control form-control-sm" id="subFileInput" /><input type="hidden" id="subFilePath" /></div>` : ""}
                    <button type="submit" class="btn btn-primary btn-sm mt-2">Submit</button>
                </form>`;

                if (a.timeLimitMinutes && !a.mySubmission) {
                    let secs = a.timeLimitMinutes * 60;
                    quizTimerInterval = setInterval(function () {
                        secs--;
                        const m = Math.floor(secs / 60), s = secs % 60;
                        $("#quizTimerBanner").text(`Time Remaining: ${m}:${String(s).padStart(2, "0")}`);
                        if (secs <= 0) {
                            clearInterval(quizTimerInterval);
                            $("#studentSubmitActivityForm").trigger("submit");
                        }
                    }, 1000);
                }
            }

            $("#actDetailBody").html(body);
            activityModal.show();
        }).fail(xhr => showToast("Unavailable", xhr.responseJSON?.message || "Cannot open activity.", true));
    });

    $(document).on("change", "#subFileInput", function () {
        if (!this.files[0]) return;
        const fd = new FormData();
        fd.append("file", this.files[0]);
        $.ajax({
            url: "/api/workspace/upload",
            method: "POST",
            data: fd,
            processData: false,
            contentType: false,
            success: res => $("#subFilePath").val(res.filePath)
        });
    });

    $(document).on("submit", "#studentSubmitActivityForm", function (e) {
        e.preventDefault();
        clearInterval(quizTimerInterval);
        const aId = $(this).data("act-id");
        const answers = {};
        $(".q-answer-box").each(function () {
            const qId = $(this).data("q-id");
            const radioVal = $(this).find(".q-ans-radio:checked").val();
            const textVal = $(this).find(".q-ans-input").val();
            if (radioVal || textVal) answers[qId] = radioVal || textVal;
        });

        $.ajax({
            url: `/api/workspace/activities/${aId}/submit`,
            method: "POST",
            contentType: "application/json",
            data: JSON.stringify({
                submissionText: $("#subText").val() || null,
                attachmentPath: $("#subFilePath").val() || null,
                answers
            }),
            success: res => {
                activityModal.hide();
                showToast("Submitted", res.isAutoEvaluated ? `Auto-graded: ${res.obtainedMarks} marks.` : "Submission received.", false);
                loadWorkspace();
            },
            error: xhr => showToast("Error", xhr.responseJSON?.message || "Submission failed.", true)
        });
    });

    $(document).on("submit", "#forumReplyForm", function (e) {
        e.preventDefault();
        const aId = $(this).data("act-id");
        $.ajax({
            url: `/api/workspace/activities/${aId}/forum`,
            method: "POST",
            contentType: "application/json",
            data: JSON.stringify({ content: $("#forumContent").val() }),
            success: () => { activityModal.hide(); showToast("Posted", "Comment added to forum.", false); }
        });
    });

    loadWorkspace();
});

