$(function () {
    const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    const userId = $("#studentForm").data("user-id");
    const isEdit = Boolean(userId);

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

    function setErr($el, $err, msg) { $el.addClass("is-invalid"); $err.text(msg); }
    function clrErr($el, $err) { $el.removeClass("is-invalid"); $err.text(""); }

    function validateRequired($el, $err, label, minLen = 1) {
        const v = ($el.val() || "").trim();
        if (!v) { setErr($el, $err, `${label} is required.`); return false; }
        if (v.length < minLen) { setErr($el, $err, `${label} must be at least ${minLen} characters.`); return false; }
        clrErr($el, $err); return true;
    }

    function validateEmail() {
        const $el = $("#studentEmail"), $err = $("#studentEmailError");
        const v = ($el.val() || "").trim();
        if (!v) { setErr($el, $err, "Email is required."); return false; }
        if (!emailPattern.test(v)) { setErr($el, $err, "Please enter a valid email address."); return false; }
        clrErr($el, $err); return true;
    }

    function validatePasswords($p, $pe, $c, $ce) {
        const p = $p.val() || "", c = $c.val() || "";
        let ok = true;
        if (!p) { setErr($p, $pe, "Password is required."); ok = false; }
        else if (p.length < 8) { setErr($p, $pe, "Password must be at least 8 characters."); ok = false; }
        else { clrErr($p, $pe); }

        if (!c) { setErr($c, $ce, "Confirm Password is required."); ok = false; }
        else if (c !== p) { setErr($c, $ce, "Confirm Password must match Password."); ok = false; }
        else { clrErr($c, $ce); }
        return ok;
    }

    $(".status-pill-btn").on("click", function () {
        const val = $(this).data("status-val").toString();
        $("#studentStatus").val(val);
        $(".status-pill-btn").removeClass("active-status inactive-status");
        $(this).addClass(val === "1" ? "active-status" : "inactive-status");
    });

    $("#studentPhotoFile").on("change", function () {
        const file = this.files[0];
        if (!file) return;
        const fd = new FormData();
        fd.append("file", file);
        $.ajax({
            url: "/api/admin/uploads/students",
            method: "POST",
            data: fd,
            processData: false,
            contentType: false,
            success: function (res) {
                $("#studentImagePath").val(res.imagePath);
                $("#studentPhotoPreview").attr("src", res.imagePath);
                showToast("Photo Uploaded", "Profile photo ready to save.", false);
            },
            error: function (xhr) {
                showToast("Upload Error", xhr.responseJSON?.message || "Failed to upload photo.", true);
            }
        });
    });

    if (isEdit) {
        $.get(`/api/admin/users/${userId}`).done(function (u) {
            $("#studentIdInput").val(u.studentId || "");
            $("#studentDepartment").val(u.department || "");
            $("#studentFullName").val(u.fullName || "");
            $("#studentEmail").val(u.email || "");
            $("#studentPhone").val(u.phoneNumber || "");
            $("#studentImagePath").val(u.imagePath || "");
            if (u.imagePath) $("#studentPhotoPreview").attr("src", u.imagePath);
            const st = u.status === "Inactive" ? "2" : "1";
            $("#studentStatus").val(st);
            $(".status-pill-btn").removeClass("active-status inactive-status");
            $(`.status-pill-btn[data-status-val='${st}']`).addClass(st === "1" ? "active-status" : "inactive-status");
        });
    }

    $("#studentForm").on("submit", function (e) {
        e.preventDefault();
        const v1 = validateRequired($("#studentIdInput"), $("#studentIdInputError"), "Student ID");
        const v2 = validateRequired($("#studentFullName"), $("#studentFullNameError"), "Full Name", 2);
        const v3 = validateEmail();
        const v4 = validateRequired($("#studentPhone"), $("#studentPhoneError"), "Phone Number");
        const v5 = isEdit ? true : validatePasswords($("#studentPassword"), $("#studentPasswordError"), $("#studentConfirmPassword"), $("#studentConfirmPasswordError"));

        if (!v1 || !v2 || !v3 || !v4 || !v5) {
            showToast("Form Validation Error", "Please correct the highlighted fields before saving.", true);
            return;
        }

        const payload = {
            studentId: ($("#studentIdInput").val() || "").trim(),
            department: ($("#studentDepartment").val() || "").trim() || null,
            fullName: ($("#studentFullName").val() || "").trim(),
            email: ($("#studentEmail").val() || "").trim(),
            phoneNumber: ($("#studentPhone").val() || "").trim(),
            status: parseInt($("#studentStatus").val(), 10),
            imagePath: ($("#studentImagePath").val() || "").trim() || null
        };

        if (!isEdit) {
            payload.password = $("#studentPassword").val();
            payload.confirmPassword = $("#studentConfirmPassword").val();
        }

        $.ajax({
            url: isEdit ? `/api/admin/users/student/${userId}` : "/api/admin/users/student",
            method: isEdit ? "PUT" : "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function () {
                window.location.href = "/Admin/Dashboard#students";
            },
            error: function (xhr) {
                showToast("Save Error", xhr.responseJSON?.message || "Unable to save student.", true);
            }
        });
    });

    $("#studentResetPasswordForm").on("submit", function (e) {
        e.preventDefault();
        if (!validatePasswords($("#resetNewPassword"), $("#resetNewPasswordError"), $("#resetConfirmPassword"), $("#resetConfirmPasswordError"))) {
            showToast("Validation Error", "Please check the password fields.", true);
            return;
        }
        $.ajax({
            url: `/api/admin/users/${userId}/reset-password`,
            method: "POST",
            contentType: "application/json",
            data: JSON.stringify({
                newPassword: $("#resetNewPassword").val(),
                confirmPassword: $("#resetConfirmPassword").val()
            }),
            success: function () {
                $("#studentResetPasswordForm")[0].reset();
                showToast("Password Reset", "Student password updated successfully.", false);
            },
            error: function (xhr) {
                showToast("Reset Error", xhr.responseJSON?.message || "Failed to reset password.", true);
            }
        });
    });
});

