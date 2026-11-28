$(function () {
    const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    const userId = $("#teacherForm").data("user-id");
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

    function validateRequired($el, $err, label) {
        const v = ($el.val() || "").trim();
        if (!v) { setErr($el, $err, `${label} is required.`); return false; }
        clrErr($el, $err); return true;
    }

    function validateEmail() {
        const $el = $("#teacherEmail"), $err = $("#teacherEmailError");
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
        $("#teacherStatus").val(val);
        $(".status-pill-btn").removeClass("active-status inactive-status");
        $(this).addClass(val === "1" ? "active-status" : "inactive-status");
    });

    $("#teacherPhotoFile").on("change", function () {
        const file = this.files[0];
        if (!file) return;
        const fd = new FormData();
        fd.append("file", file);
        $.ajax({
            url: "/api/admin/uploads/teachers",
            method: "POST",
            data: fd,
            processData: false,
            contentType: false,
            success: function (res) {
                $("#teacherImagePath").val(res.imagePath);
                $("#teacherPhotoPreview").attr("src", res.imagePath);
                showToast("Photo Uploaded", "Profile photo ready to save.", false);
            },
            error: function (xhr) {
                showToast("Upload Error", xhr.responseJSON?.message || "Failed to upload photo.", true);
            }
        });
    });

    if (isEdit) {
        $.get(`/api/admin/users/${userId}`).done(function (u) {
            $("#teacherFullName").val(u.fullName || "");
            $("#teacherEmail").val(u.email || "");
            $("#teacherPhone").val(u.phoneNumber || "");
            $("#teacherImagePath").val(u.imagePath || "");
            if (u.imagePath) $("#teacherPhotoPreview").attr("src", u.imagePath);
            const st = u.status === "Inactive" ? "2" : "1";
            $("#teacherStatus").val(st);
            $(".status-pill-btn").removeClass("active-status inactive-status");
            $(`.status-pill-btn[data-status-val='${st}']`).addClass(st === "1" ? "active-status" : "inactive-status");
        });
    }

    $("#teacherForm").on("submit", function (e) {
        e.preventDefault();
        const v1 = validateRequired($("#teacherFullName"), $("#teacherFullNameError"), "Full Name");
        const v2 = validateEmail();
        const v3 = isEdit ? true : validatePasswords($("#teacherPassword"), $("#teacherPasswordError"), $("#teacherConfirmPassword"), $("#teacherConfirmPasswordError"));

        if (!v1 || !v2 || !v3) {
            showToast("Form Validation Error", "Please correct the highlighted fields before saving.", true);
            return;
        }

        const payload = {
            fullName: ($("#teacherFullName").val() || "").trim(),
            email: ($("#teacherEmail").val() || "").trim(),
            phoneNumber: ($("#teacherPhone").val() || "").trim() || null,
            status: parseInt($("#teacherStatus").val(), 10),
            imagePath: ($("#teacherImagePath").val() || "").trim() || null
        };

        if (!isEdit) {
            payload.password = $("#teacherPassword").val();
            payload.confirmPassword = $("#teacherConfirmPassword").val();
        }

        $.ajax({
            url: isEdit ? `/api/admin/users/teacher/${userId}` : "/api/admin/users/teacher",
            method: isEdit ? "PUT" : "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function () {
                window.location.href = "/Admin/Dashboard#teachers";
            },
            error: function (xhr) {
                showToast("Save Error", xhr.responseJSON?.message || "Unable to save teacher.", true);
            }
        });
    });

    $("#teacherResetPasswordForm").on("submit", function (e) {
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
                $("#teacherResetPasswordForm")[0].reset();
                showToast("Password Reset", "Teacher password updated successfully.", false);
            },
            error: function (xhr) {
                showToast("Reset Error", xhr.responseJSON?.message || "Failed to reset password.", true);
            }
        });
    });
});

