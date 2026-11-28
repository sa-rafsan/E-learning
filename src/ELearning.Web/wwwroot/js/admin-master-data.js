(function () {
    let departmentsCache = [];
    let batchesCache = [];
    let sectionsCache = [];
    let semestersCache = [];

    function escapeHtml(text) {
        return $("<div>").text(text ?? "").html();
    }

    function showMasterAlert(message, type) {
        $("#masterDataAlert").html(`
            <div class="alert alert-${type} alert-dismissible fade show py-2 small" role="alert">
                <span>${escapeHtml(message)}</span>
                <button type="button" class="btn-close btn-sm" data-bs-dismiss="alert" aria-label="Close"></button>
            </div>`);
    }

    function ensureDatalist(inputSelector, listId, items, labelFn) {
        const $input = $(inputSelector);
        if (!$input.length) return;
        let $datalist = $(`#${listId}`);
        if (!$datalist.length) {
            $datalist = $(`<datalist id="${listId}"></datalist>`).insertAfter($input);
        }
        $input.attr("list", listId);
        $datalist.html(items.map(item => `<option value="${escapeHtml(labelFn(item))}"></option>`).join(""));
    }

    function syncDropdowns() {
        const $stuDept = $("#studentDepartment");
        if ($stuDept.length && $stuDept.is("select")) {
            const currentVal = $stuDept.val();
            const opts = departmentsCache.map(d =>
                `<option value="${escapeHtml(d.name)} (${escapeHtml(d.code)})">${escapeHtml(d.name)} (${escapeHtml(d.code)})</option>`
            ).join("");
            $stuDept.html(`<option value="">Select Department...</option>${opts}`);
            if (currentVal) $stuDept.val(currentVal);
        }

        const $teaDept = $("#teacherDepartment");
        if ($teaDept.length && $teaDept.is("select")) {
            const currentVal = $teaDept.val();
            const opts = departmentsCache.map(d =>
                `<option value="${escapeHtml(d.name)} (${escapeHtml(d.code)})">${escapeHtml(d.name)} (${escapeHtml(d.code)})</option>`
            ).join("");
            $teaDept.html(`<option value="">Select Department...</option>${opts}`);
            if (currentVal) $teaDept.val(currentVal);
        }

        const $stuBatch = $("#studentBatch");
        if ($stuBatch.length && $stuBatch.is("select")) {
            const currentBatch = $stuBatch.val();
            const bOpts = batchesCache.map(b =>
                `<option value="${escapeHtml(b.name)}">${escapeHtml(b.name)} (${escapeHtml(b.code)})</option>`
            ).join("");
            $stuBatch.html(`<option value="">Select Batch...</option>${bOpts}`);
            if (currentBatch) $stuBatch.val(currentBatch);
        }

        const $stuSec = $("#studentSection");
        if ($stuSec.length && $stuSec.is("select")) {
            const currentSec = $stuSec.val();
            const sOpts = sectionsCache.map(s =>
                `<option value="${escapeHtml(s.name)}">${escapeHtml(s.name)}</option>`
            ).join("");
            $stuSec.html(`<option value="">Select Section...</option>${sOpts}`);
            if (currentSec) $stuSec.val(currentSec);
        }

        const $stuSem = $("#studentSemester");
        if ($stuSem.length && $stuSem.is("select")) {
            const currentSem = $stuSem.val();
            const semOpts = semestersCache.map(s =>
                `<option value="${escapeHtml(s.name)}">${escapeHtml(s.name)}</option>`
            ).join("");
            $stuSem.html(`<option value="">Select Semester...</option>${semOpts}`);
            if (currentSem) $stuSem.val(currentSem);
        }

        ensureDatalist("#courseCategory", "masterCategoriesList", departmentsCache, d => d.name);
        ensureDatalist("#editCourseCategory", "masterEditCategoriesList", departmentsCache, d => d.name);
        ensureDatalist("#manageBatchNameInput", "masterBatchNamesList", batchesCache, b => b.name);
        ensureDatalist("#manageBatchCodeInput", "masterBatchCodesList", batchesCache, b => b.code);

        const $batchSecMenu = $("#masterBatchSectionsMenu");
        if ($batchSecMenu.length) {
            const selected = getSelectedBatchSections();
            const selectedSet = new Set(selected);
            $batchSecMenu.html(sectionsCache.map(s => `
                <div class="form-check py-1">
                    <input class="form-check-input master-batch-sec-cb" type="checkbox" value="${escapeHtml(s.name)}" id="mbSec_${s.id}" ${selectedSet.has(s.name) ? "checked" : ""} />
                    <label class="form-check-label small w-100" for="mbSec_${s.id}">${escapeHtml(s.name)}</label>
                </div>`).join(""));
            updateBatchSectionsLabel();
        }
    }

    function getSelectedBatchSections() {
        const vals = [];
        $(".master-batch-sec-cb:checked").each(function () {
            vals.push($(this).val());
        });
        return vals;
    }

    function setSelectedBatchSections(sections) {
        const set = new Set(sections || []);
        $(".master-batch-sec-cb").each(function () {
            $(this).prop("checked", set.has($(this).val()));
        });
        updateBatchSectionsLabel();
    }

    function updateBatchSectionsLabel() {
        const vals = getSelectedBatchSections();
        $("#masterBatchSectionsLabel").text(vals.length ? vals.join(", ") : "Select Sections...");
    }

    const EDIT_ICON = `<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>`;
    const REMOVE_ICON = `<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><polyline points="3 6 5 6 21 6"/><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"/></svg>`;

    function renderDepartmentsTable() {
        const $tbody = $("#departmentsTableBody");
        if (!$tbody.length) return;
        if (!departmentsCache.length) {
            $tbody.html('<tr><td colspan="3" class="text-center py-3 text-muted small">No departments added yet.</td></tr>');
            return;
        }
        $tbody.html(departmentsCache.map(d => `
            <tr>
                <td class="fw-semibold">${escapeHtml(d.name)}</td>
                <td><span class="badge bg-light text-dark border">${escapeHtml(d.code)}</span></td>
                <td class="text-end text-nowrap">
                    <button type="button" class="btn btn-sm btn-outline-primary btn-edit-dept me-1" data-id="${d.id}" title="Edit" aria-label="Edit">${EDIT_ICON}</button>
                    <button type="button" class="btn btn-sm btn-outline-danger btn-delete-dept" data-id="${d.id}" title="Remove" aria-label="Remove">${REMOVE_ICON}</button>
                </td>
            </tr>`).join(""));
    }

    function renderBatchesTable() {
        const $tbody = $("#masterBatchesTableBody");
        if (!$tbody.length) return;
        if (!batchesCache.length) {
            $tbody.html('<tr><td colspan="4" class="text-center py-3 text-muted small">No batches added yet.</td></tr>');
            return;
        }
        $tbody.html(batchesCache.map(b => {
            const secBadges = (b.sections || ["A", "B", "C", "D"]).map(s =>
                `<span class="badge bg-light text-dark border me-1">${escapeHtml(s)}</span>`
            ).join("");
            return `
            <tr>
                <td class="fw-semibold">${escapeHtml(b.name)}</td>
                <td><span class="badge bg-info-subtle text-info-emphasis">${escapeHtml(b.code)}</span></td>
                <td>${secBadges}</td>
                <td class="text-end text-nowrap">
                    <button type="button" class="btn btn-sm btn-outline-primary btn-edit-master-batch me-1" data-id="${b.id}" title="Edit" aria-label="Edit">${EDIT_ICON}</button>
                    <button type="button" class="btn btn-sm btn-outline-danger btn-delete-master-batch" data-id="${b.id}" title="Remove" aria-label="Remove">${REMOVE_ICON}</button>
                </td>
            </tr>`;
        }).join(""));
    }

    function renderSectionsTable() {
        const $tbody = $("#masterSectionsTableBody");
        if (!$tbody.length) return;
        if (!sectionsCache.length) {
            $tbody.html('<tr><td colspan="2" class="text-center py-3 text-muted small">No sections added yet.</td></tr>');
            return;
        }
        $tbody.html(sectionsCache.map(s => `
            <tr>
                <td class="fw-semibold">${escapeHtml(s.name)}</td>
                <td class="text-end text-nowrap">
                    <button type="button" class="btn btn-sm btn-outline-primary btn-edit-section me-1" data-id="${s.id}" title="Edit" aria-label="Edit">${EDIT_ICON}</button>
                    <button type="button" class="btn btn-sm btn-outline-danger btn-delete-section" data-id="${s.id}" title="Remove" aria-label="Remove">${REMOVE_ICON}</button>
                </td>
            </tr>`).join(""));
    }

    function renderSemestersTable() {
        const $tbody = $("#masterSemestersTableBody");
        if (!$tbody.length) return;
        if (!semestersCache.length) {
            $tbody.html('<tr><td colspan="2" class="text-center py-3 text-muted small">No semesters added yet.</td></tr>');
            return;
        }
        $tbody.html(semestersCache.map(s => `
            <tr>
                <td class="fw-semibold">${escapeHtml(s.name)}</td>
                <td class="text-end text-nowrap">
                    <button type="button" class="btn btn-sm btn-outline-primary btn-edit-semester me-1" data-id="${s.id}" title="Edit" aria-label="Edit">${EDIT_ICON}</button>
                    <button type="button" class="btn btn-sm btn-outline-danger btn-delete-semester" data-id="${s.id}" title="Remove" aria-label="Remove">${REMOVE_ICON}</button>
                </td>
            </tr>`).join(""));
    }

    function loadMasterData() {
        return $.when(
            $.get("/api/admin/master-data/departments"),
            $.get("/api/admin/master-data/batches"),
            $.get("/api/admin/master-data/sections"),
            $.get("/api/admin/master-data/semesters")
        ).done(function (deptRes, batchRes, secRes, semRes) {
            departmentsCache = deptRes[0] || [];
            batchesCache = batchRes[0] || [];
            sectionsCache = secRes[0] || [];
            semestersCache = semRes[0] || [];
            renderDepartmentsTable();
            renderBatchesTable();
            renderSectionsTable();
            renderSemestersTable();
            syncDropdowns();
        });
    }

    function resetDeptForm() {
        $("#editDeptId").val("");
        $("#createDepartmentForm")[0].reset();
        $("#deptSubmitBtn").text("+ Add");
        $("#deptCancelEditBtn").addClass("d-none");
    }

    function resetBatchForm() {
        $("#editMasterBatchId").val("");
        $("#createMasterBatchForm")[0].reset();
        setSelectedBatchSections([]);
        $("#masterBatchSubmitBtn").text("+ Add");
        $("#masterBatchCancelEditBtn").addClass("d-none");
    }

    window.AdminMasterData = {
        getDepartments: () => departmentsCache,
        getBatches: () => batchesCache,
        getSections: () => sectionsCache,
        syncDropdowns,
        reload: loadMasterData
    };

    $(function () {
        loadMasterData();

        $("#createDepartmentForm").on("submit", function (e) {
            e.preventDefault();
            const editId = $("#editDeptId").val();
            const name = ($("#deptNameInput").val() || "").trim();
            const code = ($("#deptCodeInput").val() || "").trim().toUpperCase();
            if (!name || !code) return;

            const url = editId ? `/api/admin/master-data/departments/${editId}` : "/api/admin/master-data/departments";
            const method = editId ? "PUT" : "POST";

            $.ajax({
                url,
                method,
                contentType: "application/json",
                data: JSON.stringify({ name, code }),
                success: function () {
                    showMasterAlert(`Department '${name}' ${editId ? "updated" : "added"}.`, "success");
                    resetDeptForm();
                    loadMasterData();
                },
                error: function (xhr) {
                    showMasterAlert(xhr.responseJSON?.message || "Failed to save department.", "danger");
                }
            });
        });

        $(document).on("click", ".btn-edit-dept", function () {
            const id = $(this).data("id");
            const dept = departmentsCache.find(d => d.id === id);
            if (!dept) return;
            $("#editDeptId").val(dept.id);
            $("#deptNameInput").val(dept.name);
            $("#deptCodeInput").val(dept.code);
            $("#deptSubmitBtn").text("Save");
            $("#deptCancelEditBtn").removeClass("d-none");
        });

        $("#deptCancelEditBtn").on("click", resetDeptForm);

        $(document).on("click", ".btn-delete-dept", function () {
            const id = $(this).data("id");
            $.ajax({
                url: `/api/admin/master-data/departments/${id}`,
                method: "DELETE",
                success: function (res) {
                    showMasterAlert(res.message || "Department removed.", "info");
                    loadMasterData();
                },
                error: function (xhr) {
                    showMasterAlert(xhr.responseJSON?.message || "Cannot remove department.", "warning");
                }
            });
        });

        $(document).on("change", ".master-batch-sec-cb", updateBatchSectionsLabel);

        $("#createMasterBatchForm").on("submit", function (e) {
            e.preventDefault();
            const editId = $("#editMasterBatchId").val();
            const name = ($("#masterBatchNameInput").val() || "").trim();
            const code = ($("#masterBatchCodeInput").val() || "").trim().toUpperCase();
            const sections = getSelectedBatchSections();
            if (!name || !code) return;

            const url = editId ? `/api/admin/master-data/batches/${editId}` : "/api/admin/master-data/batches";
            const method = editId ? "PUT" : "POST";

            $.ajax({
                url,
                method,
                contentType: "application/json",
                data: JSON.stringify({ name, code, sections }),
                success: function () {
                    showMasterAlert(`Batch '${name}' ${editId ? "updated" : "added"}.`, "success");
                    resetBatchForm();
                    loadMasterData();
                },
                error: function (xhr) {
                    showMasterAlert(xhr.responseJSON?.message || "Failed to save batch.", "danger");
                }
            });
        });

        $(document).on("click", ".btn-edit-master-batch", function () {
            const id = $(this).data("id");
            const batch = batchesCache.find(b => b.id === id);
            if (!batch) return;
            $("#editMasterBatchId").val(batch.id);
            $("#masterBatchNameInput").val(batch.name);
            $("#masterBatchCodeInput").val(batch.code);
            setSelectedBatchSections(batch.sections || []);
            $("#masterBatchSubmitBtn").text("Save");
            $("#masterBatchCancelEditBtn").removeClass("d-none");
        });

        $("#masterBatchCancelEditBtn").on("click", resetBatchForm);

        $(document).on("click", ".btn-delete-master-batch", function () {
            const id = $(this).data("id");
            $.ajax({
                url: `/api/admin/master-data/batches/${id}`,
                method: "DELETE",
                success: function (res) {
                    showMasterAlert(res.message || "Batch removed.", "info");
                    loadMasterData();
                },
                error: function (xhr) {
                    showMasterAlert(xhr.responseJSON?.message || "Cannot remove batch.", "warning");
                }
            });
        });

        function resetSectionForm() {
            $("#editSectionId").val("");
            $("#createSectionForm")[0].reset();
            $("#sectionSubmitBtn").text("+ Add");
            $("#sectionCancelEditBtn").addClass("d-none");
        }

        $("#createSectionForm").on("submit", function (e) {
            e.preventDefault();
            const editId = $("#editSectionId").val();
            const name = ($("#sectionNameInput").val() || "").trim().toUpperCase();
            if (!name) return;

            const url = editId ? `/api/admin/master-data/sections/${editId}` : "/api/admin/master-data/sections";
            const method = editId ? "PUT" : "POST";

            $.ajax({
                url,
                method,
                contentType: "application/json",
                data: JSON.stringify({ name }),
                success: function () {
                    showMasterAlert(`Section '${name}' ${editId ? "updated" : "added"}.`, "success");
                    resetSectionForm();
                    loadMasterData();
                },
                error: function (xhr) {
                    showMasterAlert(xhr.responseJSON?.message || "Failed to save section.", "danger");
                }
            });
        });

        $(document).on("click", ".btn-edit-section", function () {
            const id = $(this).data("id");
            const sec = sectionsCache.find(s => s.id === id);
            if (!sec) return;
            $("#editSectionId").val(sec.id);
            $("#sectionNameInput").val(sec.name);
            $("#sectionSubmitBtn").text("Save");
            $("#sectionCancelEditBtn").removeClass("d-none");
        });

        $("#sectionCancelEditBtn").on("click", resetSectionForm);

        $(document).on("click", ".btn-delete-section", function () {
            const id = $(this).data("id");
            $.ajax({
                url: `/api/admin/master-data/sections/${id}`,
                method: "DELETE",
                success: function (res) {
                    showMasterAlert(res.message || "Section removed.", "info");
                    loadMasterData();
                },
                error: function (xhr) {
                    showMasterAlert(xhr.responseJSON?.message || "Cannot remove section.", "warning");
                }
            });
        });

        function resetSemesterForm() {
            $("#editSemesterId").val("");
            $("#createSemesterForm")[0].reset();
            $("#semesterSubmitBtn").text("+ Add");
            $("#semesterCancelEditBtn").addClass("d-none");
        }

        $("#createSemesterForm").on("submit", function (e) {
            e.preventDefault();
            const editId = $("#editSemesterId").val();
            const name = ($("#semesterNameInput").val() || "").trim();
            if (!name) return;

            const url = editId ? `/api/admin/master-data/semesters/${editId}` : "/api/admin/master-data/semesters";
            const method = editId ? "PUT" : "POST";

            $.ajax({
                url,
                method,
                contentType: "application/json",
                data: JSON.stringify({ name }),
                success: function () {
                    showMasterAlert(`Semester '${name}' ${editId ? "updated" : "added"}.`, "success");
                    resetSemesterForm();
                    loadMasterData();
                },
                error: function (xhr) {
                    showMasterAlert(xhr.responseJSON?.message || "Failed to save semester.", "danger");
                }
            });
        });

        $(document).on("click", ".btn-edit-semester", function () {
            const id = $(this).data("id");
            const sem = semestersCache.find(s => s.id === id);
            if (!sem) return;
            $("#editSemesterId").val(sem.id);
            $("#semesterNameInput").val(sem.name);
            $("#semesterSubmitBtn").text("Save");
            $("#semesterCancelEditBtn").removeClass("d-none");
        });

        $("#semesterCancelEditBtn").on("click", resetSemesterForm);

        $(document).on("click", ".btn-delete-semester", function () {
            const id = $(this).data("id");
            $.ajax({
                url: `/api/admin/master-data/semesters/${id}`,
                method: "DELETE",
                success: function (res) {
                    showMasterAlert(res.message || "Semester removed.", "info");
                    loadMasterData();
                },
                error: function (xhr) {
                    showMasterAlert(xhr.responseJSON?.message || "Cannot remove semester.", "warning");
                }
            });
        });
    });
})();
