$(function () {
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

    function escapeHtml(text) {
        return $("<div>").text(text ?? "").html();
    }

    $.get("/api/account/profile").done(function (p) {
        if (p.imagePath) {
            $("#profilePhotoPreview").attr("src", p.imagePath);
        }
        $("#profileDetailsList").html(`
            <dt class="col-sm-4 text-muted">Full Name</dt><dd class="col-sm-8 fw-semibold">${escapeHtml(p.fullName)}</dd>
            <dt class="col-sm-4 text-muted">Email</dt><dd class="col-sm-8">${escapeHtml(p.email)}</dd>
            <dt class="col-sm-4 text-muted">Role</dt><dd class="col-sm-8">${escapeHtml(p.role)}</dd>
            <dt class="col-sm-4 text-muted">Phone</dt><dd class="col-sm-8">${escapeHtml(p.phoneNumber || "-")}</dd>
            ${p.studentId ? `<dt class="col-sm-4 text-muted">Student ID</dt><dd class="col-sm-8">${escapeHtml(p.studentId)}</dd>` : ""}
            ${p.department ? `<dt class="col-sm-4 text-muted">Department</dt><dd class="col-sm-8">${escapeHtml(p.department)}</dd>` : ""}
            <dt class="col-sm-4 text-muted">Status</dt><dd class="col-sm-8">${escapeHtml(p.status)}</dd>
        `);
    });

    $("#profilePhotoInput").on("change", function () {
        const file = this.files[0];
        if (!file) return;
        const fd = new FormData();
        fd.append("file", file);
        $.ajax({
            url: "/api/account/profile/photo",
            method: "POST",
            data: fd,
            processData: false,
            contentType: false,
            success: function (res) {
                $("#profilePhotoPreview").attr("src", res.imagePath);
                showToast("Photo Updated", "Your profile photo has been saved.", false);
            },
            error: function (xhr) {
                showToast("Upload Error", xhr.responseJSON?.message || "Failed to upload photo.", true);
            }
        });
    });

    $("#selfChangePasswordForm").on("submit", function (e) {
        e.preventDefault();
        const cur = $("#selfCurrentPassword").val() || "";
        const np = $("#selfNewPassword").val() || "";
        const cp = $("#selfConfirmPassword").val() || "";

        let ok = true;
        if (!cur) { $("#selfCurrentPassword").addClass("is-invalid"); $("#selfCurrentPasswordError").text("Current Password is required."); ok = false; }
        else { $("#selfCurrentPassword").removeClass("is-invalid"); }

        if (!np || np.length < 8) { $("#selfNewPassword").addClass("is-invalid"); $("#selfNewPasswordError").text("New Password must be at least 8 characters."); ok = false; }
        else { $("#selfNewPassword").removeClass("is-invalid"); }

        if (!cp || cp !== np) { $("#selfConfirmPassword").addClass("is-invalid"); $("#selfConfirmPasswordError").text("Confirm Password must match New Password."); ok = false; }
        else { $("#selfConfirmPassword").removeClass("is-invalid"); }

        if (!ok) return;

        $.ajax({
            url: "/api/account/change-password",
            method: "POST",
            contentType: "application/json",
            data: JSON.stringify({
                currentPassword: cur,
                newPassword: np,
                confirmPassword: cp
            }),
            success: function () {
                $("#selfChangePasswordForm")[0].reset();
                showToast("Password Updated", "Your password has been changed successfully.", false);
            },
            error: function (xhr) {
                showToast("Update Failed", xhr.responseJSON?.message || "Unable to update password.", true);
            }
        });
    });
});
