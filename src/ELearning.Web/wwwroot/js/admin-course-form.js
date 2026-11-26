$(function () {
    const courseId = $("#courseForm").data("course-id");
    const isEdit = Boolean(courseId);

    function showToast(title, message, isError) {
        $("#adminToastContainer").html(`
            <div class="admin-toast-card ${isError ? "" : "toast-success"}" role="alert">
                <div class="fw-bold ${isError ? "text-danger" : "text-success"}">${isError ? "&#9888;" : "&#10003;"}</div>
                <div class="flex-grow-1">
                    <div class="fw-bold small text-dark">${$("<div>").text(title).html()}</div>
                    <div class="small text-muted">${$("<div>").text(message).html()}</div>
                </div>
                <button type="button" class="btn-close btn-sm" onclick="$('#adminToastContainer').empty()"></button>
            </div>`);
        setTimeout(() => $("#adminToastContainer").empty(), 4500);
    }

    $("#coursePhotoFile").on("change", function () {
        const file = this.files[0];
        if (!file) return;
        const fd = new FormData();
        fd.append("file", file);
        $.ajax({
            url: "/api/admin/uploads/courses",
            method: "POST",
            data: fd,
            processData: false,
            contentType: false,
            success: function (res) {
                $("#courseImagePath").val(res.imagePath);
                $("#coursePhotoPreview").attr("src", res.imagePath);
                showToast("Image Uploaded", "Course cover image ready to save.", false);
            },
            error: function (xhr) {
                showToast("Upload Error", xhr.responseJSON?.message || "Failed to upload image.", true);
            }
        });
    });

    $.get("/api/admin/courses/options").done(function (data) {
        const opts = (data.teachers || []).map(t => `<option value="${t.id}">${$("<div>").text(t.fullName).html()}</option>`).join("");
        $("#courseTeacher").html(`<option value="">-- Assign Later --</option>${opts}`);

        if (isEdit) {
            $.get(`/api/admin/courses/${courseId}`).done(function (c) {
                $("#courseTitle").val(c.title || "");
                $("#courseCategory").val(c.category || "");
                $("#courseDuration").val(c.durationHours || 1);
                $("#courseDescription").val(c.description || "");
                $("#courseIsPublished").prop("checked", Boolean(c.isPublished));
                $("#courseImagePath").val(c.imagePath || "");
                if (c.imagePath) $("#coursePhotoPreview").attr("src", c.imagePath);
                if (c.teacherId) $("#courseTeacher").val(c.teacherId);
            });
        }
    });

    $("#courseForm").on("submit", function (e) {
        e.preventDefault();
        const title = ($("#courseTitle").val() || "").trim();
        const category = ($("#courseCategory").val() || "").trim();
        const description = ($("#courseDescription").val() || "").trim();
        const duration = parseInt($("#courseDuration").val(), 10);

        let ok = true;
        if (!title) { $("#courseTitle").addClass("is-invalid"); $("#courseTitleError").text("Course Title is required."); ok = false; }
        else { $("#courseTitle").removeClass("is-invalid"); }
        if (!category) { $("#courseCategory").addClass("is-invalid"); $("#courseCategoryError").text("Category is required."); ok = false; }
        else { $("#courseCategory").removeClass("is-invalid"); }
        if (!description) { $("#courseDescription").addClass("is-invalid"); $("#courseDescriptionError").text("Description is required."); ok = false; }
        else { $("#courseDescription").removeClass("is-invalid"); }
        if (!duration || duration < 1) { $("#courseDuration").addClass("is-invalid"); $("#courseDurationError").text("Duration must be at least 1 hour."); ok = false; }
        else { $("#courseDuration").removeClass("is-invalid"); }

        if (!ok) {
            showToast("Form Validation Error", "Please correct the highlighted fields.", true);
            return;
        }

        const tVal = $("#courseTeacher").val();
        $.ajax({
            url: isEdit ? `/api/admin/courses/${courseId}` : "/api/admin/courses",
            method: isEdit ? "PUT" : "POST",
            contentType: "application/json",
            data: JSON.stringify({
                title,
                category,
                description,
                durationHours: duration,
                teacherId: tVal ? parseInt(tVal, 10) : null,
                isPublished: $("#courseIsPublished").is(":checked"),
                imagePath: ($("#courseImagePath").val() || "").trim() || null
            }),
            success: function () {
                window.location.href = "/Admin/Dashboard#courses";
            },
            error: function (xhr) {
                showToast("Save Error", xhr.responseJSON?.message || "Failed to save course.", true);
            }
        });
    });
});
