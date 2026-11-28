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

    function addQualRow(q) {
        const deg = escapeHtml(q?.degreeName || "");
        const inst = escapeHtml(q?.institutionName || "");
        const yr = q?.passingYear || "";
        $("#qualificationsContainer").append(`
            <div class="row g-2 qual-row align-items-center">
                <div class="col-md-5"><input type="text" class="form-control form-control-sm qual-degree" placeholder="Degree (e.g. M.Sc in CSE)" value="${deg}" /></div>
                <div class="col-md-5"><input type="text" class="form-control form-control-sm qual-inst" placeholder="Institution" value="${inst}" /></div>
                <div class="col-md-2 d-flex gap-1">
                    <input type="number" class="form-control form-control-sm qual-year" placeholder="Year" value="${yr}" />
                    <button type="button" class="btn btn-sm btn-outline-danger remove-qual-btn">&times;</button>
                </div>
            </div>`);
    }

    $(document).on("click", ".remove-qual-btn", function () { $(this).closest(".qual-row").remove(); });
    $("#addQualificationBtn").on("click", () => addQualRow({}));

    $.get("/api/account/profile").done(function (p) {
        if (p.imagePath) $("#profilePhotoPreview").attr("src", p.imagePath);
        $("#profFullName").val(p.fullName || "");
        $("#profEmail").val(p.email || "");
        $("#profPhone").val(p.phoneNumber || "");
        $("#profNationality").val(p.nationality || "");
        $("#profCountry").val(p.country || "");
        $("#profAddress").val(p.address || "");
        $("#profDob").val(p.dateOfBirth || "");
        $("#profGender").val(p.gender || "");
        $("#profEduBg").val(p.educationalBackground || "");
        $("#profSpecialization").val(p.specialization || "");
        $("#profExperience").val(p.professionalExperience || "");
        $("#profBio").val(p.shortBiography || "");

        if ($("#qualificationsContainer").length && p.qualifications) {
            p.qualifications.forEach(addQualRow);
        }
    });

    $("#selfProfileForm").on("submit", function (e) {
        e.preventDefault();
        const fullName = ($("#profFullName").val() || "").trim();
        if (!fullName) {
            showToast("Validation Error", "Full Name is required.", true);
            return;
        }

        const quals = [];
        $(".qual-row").each(function () {
            const d = ($(this).find(".qual-degree").val() || "").trim();
            const i = ($(this).find(".qual-inst").val() || "").trim();
            const y = parseInt($(this).find(".qual-year").val(), 10);
            if (d) quals.push({ degreeName: d, institutionName: i, passingYear: isNaN(y) ? null : y });
        });

        $.ajax({
            url: "/api/account/profile",
            method: "PUT",
            contentType: "application/json",
            data: JSON.stringify({
                fullName,
                phoneNumber: ($("#profPhone").val() || "").trim() || null,
                address: ($("#profAddress").val() || "").trim() || null,
                nationality: ($("#profNationality").val() || "").trim() || null,
                country: ($("#profCountry").val() || "").trim() || null,
                dateOfBirth: $("#profDob").val() || null,
                gender: $("#profGender").val() || null,
                educationalBackground: ($("#profEduBg").val() || "").trim() || null,
                specialization: ($("#profSpecialization").val() || "").trim() || null,
                professionalExperience: ($("#profExperience").val() || "").trim() || null,
                shortBiography: ($("#profBio").val() || "").trim() || null,
                qualifications: $("#qualificationsContainer").length ? quals : null
            }),
            success: () => showToast("Profile Saved", "Your profile details have been updated.", false),
            error: xhr => showToast("Save Error", xhr.responseJSON?.message || "Unable to save profile.", true)
        });
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
