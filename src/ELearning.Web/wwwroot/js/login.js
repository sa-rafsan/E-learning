$(function () {
    const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

    function setFieldError($input, $errorEl, message) {
        $input.addClass("is-invalid");
        $errorEl.text(message);
    }

    function clearFieldError($input, $errorEl) {
        $input.removeClass("is-invalid");
        $errorEl.text("");
    }

    function validateRole() {
        const $input = $("#loginRole");
        const $box = $("#loginRoleBox");
        const $error = $("#loginRoleError");
        const value = ($input.val() || "").trim();

        if (!value) {
            $box.addClass("is-invalid-box");
            $error.text("Please select a role.");
            return false;
        }

        $box.removeClass("is-invalid-box");
        $error.text("");
        return true;
    }

    function validateEmail() {
        const $input = $("#loginEmail");
        const $error = $("#loginEmailError");
        const value = ($input.val() || "").trim();

        if (!value) {
            setFieldError($input, $error, "Email is required.");
            return false;
        }

        if (!emailPattern.test(value)) {
            setFieldError($input, $error, "Please enter a valid email address.");
            return false;
        }

        clearFieldError($input, $error);
        return true;
    }

    function validatePassword() {
        const $input = $("#loginPassword");
        const $error = $("#loginPasswordError");
        const value = $input.val() || "";

        if (!value.trim()) {
            setFieldError($input, $error, "Password is required.");
            return false;
        }

        clearFieldError($input, $error);
        return true;
    }

    function setLoading(isLoading) {
        const $btn = $("#loginSubmitBtn");
        const $spinner = $("#loginSpinner");
        const $text = $("#loginBtnText");

        $btn.prop("disabled", isLoading);
        if (isLoading) {
            $spinner.removeClass("d-none");
            $text.text("Signing in...");
        } else {
            $spinner.addClass("d-none");
            $text.text("Sign In to Dashboard");
        }
    }

    $(".auth-role-btn").on("click", function () {
        const role = $(this).data("role");
        $(".auth-role-btn").removeClass("active");
        $(this).addClass("active");
        $("#loginRole").val(role).trigger("change");

        $.get("/api/auth/demo-credentials", { role })
            .done(function (creds) {
                if (creds && creds.email && creds.password) {
                    $("#loginEmail").val(creds.email).trigger("input");
                    $("#loginPassword").val(creds.password).trigger("input");
                    $("#loginErrorAlert").addClass("d-none").text("");
                }
            });
    });

    $("#togglePasswordBtn").on("click", function () {
        const $pwd = $("#loginPassword");
        const isPassword = $pwd.attr("type") === "password";
        $pwd.attr("type", isPassword ? "text" : "password");
        $(this).text(isPassword ? "Hide" : "Show");
    });

    $("#loginRole").on("change", validateRole);
    $("#loginEmail").on("input blur", validateEmail);
    $("#loginPassword").on("input blur", validatePassword);

    $("#loginForm").on("submit", function (e) {
        e.preventDefault();
        $("#loginErrorAlert").addClass("d-none").text("");

        const isRoleValid = validateRole();
        const isEmailValid = validateEmail();
        const isPasswordValid = validatePassword();

        if (!isRoleValid || !isEmailValid || !isPasswordValid) {
            return;
        }

        setLoading(true);

        const payload = {
            role: $("#loginRole").val(),
            email: ($("#loginEmail").val() || "").trim(),
            password: $("#loginPassword").val(),
            rememberMe: $("#rememberMe").is(":checked")
        };

        $.ajax({
            url: "/api/auth/login",
            method: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function (response) {
                window.location.href = response.redirectUrl || "/";
            },
            error: function (xhr) {
                setLoading(false);
                const resp = xhr.responseJSON;
                const message = (resp && resp.message)
                    ? resp.message
                    : "Unable to sign in. Please check your credentials and try again.";

                $("#loginErrorAlert").removeClass("d-none").text(message);
            }
        });
    });
});
