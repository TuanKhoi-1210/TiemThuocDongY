document.addEventListener("DOMContentLoaded", function () {
    // =====================================================
    //  TABS / PAGES
    // =====================================================
    const navItems = document.querySelectorAll(".nav-item");
    const pages = document.querySelectorAll(".page");
    const pageTitle = document.getElementById("page-title");

    function showPage(pageName, titleText) {
        if (titleText && pageTitle) {
            pageTitle.textContent = titleText;
        }

        pages.forEach(p => {
            p.classList.toggle("active", p.id === "page-" + pageName);
        });

        navItems.forEach(b => {
            const data = b.getAttribute("data-page");
            b.classList.toggle("active", data === pageName);
        });
    }

    navItems.forEach(btn => {
        btn.addEventListener("click", () => {
            const pageName = btn.getAttribute("data-page");
            const text = btn.innerText.trim();
            showPage(pageName, text);

            if (pageName === "thuoc") {
                requestThuocList();
            } else if (pageName === "khachhang") {
                requestKhachList();
            } else if (pageName === "donthuoc") {
                requestDonThuocList();
            } else if (pageName === "dashboard") {
                // sau này có thể thêm requestDashboard();
            }
        });
    });

    // =====================================================
    //  LOGIN / APP SHELL
    // =====================================================
    const loginScreen = document.getElementById("login-screen");
    const appShell = document.getElementById("app-shell");
    const btnLogin = document.getElementById("btn-login");

    window.App = window.App || {};
    App.currentUser = App.currentUser || null;
    App.userList = App.userList || [];
    App.selectedUser = null;
    App.isCreatingUser = false;
    /**
     * Xây lại dropdown "Bác sĩ kê đơn"
     * - Admin: chọn được từ danh sách bác sĩ (lấy từ App.userList)
     * - Non-admin: luôn là tên của chính mình, bị disable
     * @param {string|null} forceValue Giá trị muốn giữ lại khi đang sửa đơn (có thể null)
     */
    function refreshBacSiDropdown(forceValue) {
        const select = document.getElementById("dt-bacsi");
        if (!select || !App.currentUser) return;

        const roleName = (App.currentUser.roleName || "").toLowerCase();
        const isAdmin = roleName === "admin";

        const oldValue = forceValue != null ? forceValue : select.value;

        // reset options
        select.innerHTML = "";
        select.disabled = false;

        if (isAdmin) {
            // ===== ADMIN: có dropdown để chọn bác sĩ =====
            let doctors = [];
            if (Array.isArray(App.userList)) {
                doctors = App.userList.filter(u => {
                    const r = (u.roleName || "").toLowerCase();
                    // TODO: nếu role bác sĩ của bạn là "bán thuốc" / "doctor" / "bác sĩ" thì sửa điều kiện này cho đúng
                    return u.isActive && (r === "bác sĩ" || r === "bán thuốc");
                });
            }

            // option mặc định
            const optDefault = document.createElement("option");
            optDefault.value = "";
            optDefault.textContent = "-- Chọn thầy thuốc --";
            select.appendChild(optDefault);

            doctors.forEach(u => {
                const name = u.fullName || u.userName || "";
                if (!name) return;
                const opt = document.createElement("option");
                opt.value = name;
                opt.textContent = name;
                select.appendChild(opt);
            });

            // Nếu đang sửa đơn, giữ lại tên bác sĩ đã kê
            if (oldValue) {
                let found = Array.from(select.options).some(o => o.value === oldValue);
                if (!found) {
                    const opt = document.createElement("option");
                    opt.value = oldValue;
                    opt.textContent = oldValue;
                    select.appendChild(opt);
                }
                select.value = oldValue;
            }
            // Admin được chọn → không disable
            select.disabled = false;
        } else {
            // ===== NON-ADMIN: luôn dùng tên mình, bị khóa =====
            const name = App.currentUser.fullName || App.currentUser.userName || "";
            const opt = document.createElement("option");
            opt.value = name;
            opt.textContent = name;
            select.appendChild(opt);

            select.value = name;
            select.disabled = true;   // 🔒 luôn khóa cho non-admin
        }
    }

    // =====================================================
    //  USER DROPDOWN (TRÊN HEADER)
    // =====================================================
    const userMenu = document.querySelector(".user-menu");
    const userToggle = document.getElementById("user-menu-toggle");
    const userItems = document.querySelectorAll(".user-menu__item");

    if (userMenu && userToggle) {
        userToggle.addEventListener("click", (e) => {
            e.stopPropagation();
            userMenu.classList.toggle("open");
        });

        document.addEventListener("click", () => {
            userMenu.classList.remove("open");
        });

        userItems.forEach(item => {
            item.addEventListener("click", (e) => {
                e.stopPropagation();
                const page = item.getAttribute("data-page");
                const action = item.getAttribute("data-action");

                if (page === "taikhoan") {
                    showPage("taikhoan", "Tài khoản");
                    App.updateAccountUI(App.currentUser);
                } else if (action === "logout") {
                    App.logout();
                }

                userMenu.classList.remove("open");
            });
        });
    }

    // =====================================================
    //  MODAL OPEN / CLOSE CHUNG
    // =====================================================
    const openModalButtons = document.querySelectorAll("[data-modal-target]");
    const closeModalButtons = document.querySelectorAll("[data-modal-close]");

    openModalButtons.forEach(btn => {
        btn.addEventListener("click", () => {
            const targetId = btn.getAttribute("data-modal-target");
            const modal = document.getElementById(targetId);
            if (modal) modal.classList.add("open");
        });
    });

    closeModalButtons.forEach(btn => {
        btn.addEventListener("click", () => {
            const modal = btn.closest(".modal");
            if (modal) modal.classList.remove("open");
        });
    });

    document.querySelectorAll(".modal").forEach(modal => {
        modal.addEventListener("click", (e) => {
            if (e.target === modal) modal.classList.remove("open");
        });
    });

    // =====================================================
    //  HELPER: HEADER USER
    // =====================================================
    function updateHeaderUser(user) {
        const fullNameEl = document.getElementById("topbar-user-fullname");
        const roleEl = document.getElementById("topbar-user-role");
        const avatarEl = document.getElementById("topbar-user-avatar");

        if (!user) {
            if (fullNameEl) fullNameEl.textContent = "Chưa đăng nhập";
            if (roleEl) roleEl.textContent = "";
            if (avatarEl) avatarEl.textContent = "";
            return;
        }

        const displayName = user.fullName || user.userName || "";
        if (fullNameEl) fullNameEl.textContent = displayName;
        if (roleEl) roleEl.textContent = user.roleName || "";

        if (avatarEl) {
            const initials = (displayName || user.userName || "U")
                .split(" ")
                .filter(Boolean)
                .map(x => x[0])
                .join("")
                .substring(0, 2)
                .toUpperCase();
            avatarEl.textContent = initials;
        }
    }
    // pdf

    App.onPrintDonThuocResult = function (res) {
        if (!res) return;

        if (!res.success) {
            alert(res.message || "In đơn thuốc thất bại.");
        }
        // nếu success thì PDF đã tự mở bằng default viewer, không cần làm gì thêm
    };
    App.onPrintPhieuNhapResult = function (res) {
        if (!res) return;

        if (!res.success) {
            alert(res.message || "In phiếu nhập thất bại.");
        }
        // success: file PDF đã tự mở, không cần làm thêm gì
    };
    // =====================================================
    //  HELPER: ACCOUNT TAB (TRÊN)
    // =====================================================
    function updateAccountUI(user) {
        const uName = document.getElementById("acc-username");
        const fName = document.getElementById("acc-fullname");
        const email = document.getElementById("acc-email");
        const phone = document.getElementById("acc-phone");
        const role = document.getElementById("acc-role");

        if (!user) {
            [uName, fName, email, phone, role].forEach(el => { if (el) el.value = ""; });

            const pName = document.getElementById("acc-profile-name");
            const pRole = document.getElementById("acc-profile-role");
            const pEmail = document.getElementById("acc-profile-email");
            const pAvatar = document.getElementById("acc-profile-avatar");

            if (pName) pName.textContent = "";
            if (pRole) pRole.textContent = "";
            if (pEmail) pEmail.textContent = "";
            if (pAvatar) {
                const span = pAvatar.querySelector("span");
                if (span) span.textContent = "";
            }
            return;
        }

        if (uName) uName.value = user.userName || "";
        if (fName) fName.value = user.fullName || "";
        if (email) email.value = user.email || "";
        if (phone) phone.value = user.phoneNumber || "";

        // acc-role có thể là input hoặc select
        if (role) {
            if (role.tagName === "SELECT") {
                if (user.roleId != null) role.value = String(user.roleId);
                else role.value = "";
            } else {
                role.value = user.roleName || "";
            }
        }

        const displayName = user.fullName || user.userName || "";
        const pName = document.getElementById("acc-profile-name");
        const pRole = document.getElementById("acc-profile-role");
        const pEmail = document.getElementById("acc-profile-email");
        const pAvatar = document.getElementById("acc-profile-avatar");

        if (pName) pName.textContent = displayName;
        if (pRole) pRole.textContent = user.roleName || "";
        if (pEmail) pEmail.textContent = user.email || "";

        if (pAvatar) {
            const span = pAvatar.querySelector("span");
            if (span) {
                const initials = (displayName || user.userName || "U")
                    .split(" ")
                    .filter(Boolean)
                    .map(x => x[0])
                    .join("")
                    .substring(0, 2)
                    .toUpperCase();
                span.textContent = initials;
            }
        }
    }

    App.updateHeaderUser = updateHeaderUser;
    App.updateAccountUI = updateAccountUI;

    // =====================================================
    //  HIỆN / ẨN KHU QUẢN LÝ TÀI KHOẢN (ADMIN)
    // =====================================================
    App.setAccountAdminVisible = function (isAdmin) {
        const card = document.getElementById("account-userlist-card");
        const secHint = document.getElementById("account-security-hint");
        const buttons = document.querySelector(".account-security__buttons");

        if (!card || !secHint || !buttons) return;

        if (isAdmin) {
            card.style.display = "block";
            secHint.textContent = "Chọn một tài khoản trong danh sách bên dưới để sửa, khóa/mở hoặc xóa.";
            buttons.style.display = "flex";
        } else {
            card.style.display = "none";
            secHint.textContent = "Chức năng đổi mật khẩu, gửi mã qua email sẽ được bổ sung sau.";
            buttons.style.display = "none";
        }
    };

    // =====================================================
    //  CHO PHÉP / KHÓA CHỈNH SỬA FORM TÀI KHOẢN
    // =====================================================
    App.setAccountFormEditable = function (editable, user) {
        const isSelf = user && App.currentUser && user.userId === App.currentUser.userId;
        const isNew = App.isCreatingUser || !user;

        const ids = [
            "acc-username",
            "acc-fullname",
            "acc-email",
            "acc-phone",
            "acc-role",
            "acc-password",
            "acc-password-confirm"
        ];

        ids.forEach(id => {
            const el = document.getElementById(id);
            if (!el) return;

            if (id === "acc-username") {
                // chỉ cho sửa username khi tạo mới
                el.disabled = !isNew;
            } else if (id === "acc-role" || id === "acc-password" || id === "acc-password-confirm") {
                // không cho đổi role / password của chính mình
                el.disabled = !editable || isSelf;
            } else {
                el.disabled = !editable;
            }
        });

        const btnSave = document.getElementById("btn-account-save");
        const btnDisable = document.getElementById("btn-account-disable");
        const btnActivate = document.getElementById("btn-account-activate");
        const btnDelete = document.getElementById("btn-account-delete");
        const accNewPwd1 = document.getElementById("acc-new-password1");
        const accNewPwd2 = document.getElementById("acc-new-password2");
        const accResetCode = document.getElementById("acc-reset-code");
        const btnAccSendCode = document.getElementById("btn-account-send-code");

        if (btnAccSendCode) {
            btnAccSendCode.addEventListener("click", () => {
                const uName = (document.getElementById("acc-username")?.value || "").trim();
                if (!uName) {
                    alert("Vui lòng chọn một tài khoản hoặc nhập tên đăng nhập.");
                    return;
                }

                const payload = {
                    action: "passwordReset_request_account",
                    data: { userName: uName }
                };

                if (window.chrome && window.chrome.webview) {
                    window.chrome.webview.postMessage(payload);
                } else {
                    alert("DEMO: gửi mã đổi mật khẩu cho " + uName);
                }
            });
        }

        // Nút riêng để đổi mật khẩu bằng mã (bạn có thể dùng lại nút Lưu nếu thích)
        const btnAccResetPwd = document.getElementById("btn-account-reset-password");
        if (btnAccResetPwd) {
            btnAccResetPwd.addEventListener("click", () => {
                const uName = (document.getElementById("acc-username")?.value || "").trim();
                const code = (accResetCode?.value || "").trim();
                const p1 = (accNewPwd1?.value || "").trim();
                const p2 = (accNewPwd2?.value || "").trim();

                if (!uName) { alert("Chưa có tài khoản được chọn."); return; }
                if (!code) { alert("Vui lòng nhập mã xác nhận."); return; }
                if (!p1) { alert("Vui lòng nhập mật khẩu mới."); return; }
                if (p1 !== p2) { alert("Mật khẩu nhập lại không khớp."); return; }

                const payload = {
                    action: "passwordReset_confirm_account",
                    data: { userName: uName, code, newPassword: p1 }
                };

                if (window.chrome && window.chrome.webview) {
                    window.chrome.webview.postMessage(payload);
                } else {
                    alert("DEMO: đổi mật khẩu cho " + uName);
                }
            });
        }

        // Callback dùng cho tab Tài khoản
        App.onForgotPasswordCodeSentAccount = function (result) {
            alert(result.message || (result.success ? "Đã gửi mã tới email của tài khoản." : "Không gửi được mã."));
        };

        App.onForgotPasswordDoneAccount = function (result) {
            alert(result.message || (result.success ? "Đổi mật khẩu thành công." : "Đổi mật khẩu thất bại."));
            if (result.success) {
                if (accNewPwd1) accNewPwd1.value = "";
                if (accNewPwd2) accNewPwd2.value = "";
                if (accResetCode) accResetCode.value = "";
            }
        };

        if (btnSave) btnSave.disabled = !editable;

        if (isNew) {
            // tạo mới: không có disable / activate / delete
            if (btnDisable) { btnDisable.disabled = true; btnDisable.style.display = "none"; }
            if (btnActivate) { btnActivate.disabled = true; btnActivate.style.display = "none"; }
            if (btnDelete) btnDelete.disabled = true;
            return;
        }

        if (btnDelete) btnDelete.disabled = !editable || isSelf;

        if (btnDisable && btnActivate) {
            if (!editable || isSelf) {
                btnDisable.disabled = true;
                btnActivate.disabled = true;
                btnDisable.style.display = "inline-flex";
                btnActivate.style.display = "none";
            } else {
                const active = user && user.isActive;
                btnDisable.style.display = active ? "inline-flex" : "none";
                btnActivate.style.display = active ? "none" : "inline-flex";
                btnDisable.disabled = false;
                btnActivate.disabled = false;
            }
        }
    };

    // =====================================================
    //  LOGOUT
    // =====================================================
    App.showAppShell = function () {
        if (loginScreen) loginScreen.style.display = "none";
        if (appShell) appShell.classList.remove("app-shell-hidden");
    };

    App.logout = function () {
        App.currentUser = null;
        App.selectedUser = null;
        App.userList = [];
        App.isCreatingUser = false;

        if (appShell) appShell.classList.add("app-shell-hidden");
        if (loginScreen) loginScreen.style.display = "flex";

        const u = document.getElementById("login-username");
        const p = document.getElementById("login-password");
        if (u) u.value = "";
        if (p) p.value = "";

        App.updateHeaderUser(null);
        App.updateAccountUI(null);
        App.setAccountAdminVisible(false);
    };

    const btnLogout2 = document.getElementById("btn-logout");
    if (btnLogout2) {
        btnLogout2.addEventListener("click", () => App.logout());
    }

    // =====================================================
    //  LOGIN RESULT TỪ C#
    // =====================================================
    App.onLoginResult = function (result) {
        if (!result || !result.success) {
            alert(result && result.message ? result.message : "Đăng nhập thất bại.");
            return;
        }

        App.currentUser = {
            userId: result.userId,
            userName: result.userName,
            fullName: result.fullName,
            roleName: result.roleName,
            roleId: result.roleId,
            email: result.email,
            phoneNumber: result.phoneNumber
        };
        App.currentRole = result.roleName || "";
        App.isCreatingUser = false;
        App.selectedUser = App.currentUser;
        applyRolePermissions();
        App.updateHeaderUser(App.currentUser);
        App.updateAccountUI(App.currentUser);

        const isAdmin = (App.currentUser.roleName || "")
            .toLowerCase() === "admin";
        App.setAccountAdminVisible(isAdmin);
        App.setAccountFormEditable(false, App.currentUser);

        App.showAppShell();

        // dựng dropdown bác sĩ lần đầu (ít nhất có chính user đang login)
        refreshBacSiDropdown();

        if (isAdmin) {
            // Admin sẽ load full danh sách user -> sau đó refresh lại dropdown
            App.loadUserList();
        }
    };
    function isThuNgan() {
        const role = (App.currentRole || "").toLowerCase();
        return role.includes("thu ngân") || role.includes("thu ngan");
    }
    function applyRolePermissions() {
        const role = (App.currentRole || "").toLowerCase();
        const navButtons = document.querySelectorAll(".sidebar__nav .nav-item");

        // Mặc định cho phép tất cả
        navButtons.forEach(btn => btn.classList.remove("hidden"));

        if (role === "thu ngân" || role === "thu ngan") {
            // Thu ngân: chỉ Đơn thuốc + Nhập kho
            navButtons.forEach(btn => {
                const page = btn.dataset.page;
                if (page !== "donthuoc" && page !== "nhapkho") {
                    btn.classList.add("hidden");
                }
            });
        }
        else if (role === "bán thuốc" || role === "ban thuoc") {
            // Bán thuốc: Khách hàng + Đơn thuốc + Thuốc Đông y
            navButtons.forEach(btn => {
                const page = btn.dataset.page;
                if (page !== "khachhang" && page !== "donthuoc" && page !== "thuoc") {
                    btn.classList.add("hidden");
                }
            });

            // Ẩn nút “Thu tiền” trong Đơn thuốc
            const style = document.createElement("style");
            style.textContent = `.btn-pay-donthuoc { display: none !important; }`;
            document.head.appendChild(style);
        }
        else if (role === "admin") {
            // Admin: full quyền
            navButtons.forEach(btn => btn.classList.remove("hidden"));
        }
    }


    // =====================================================
    //  GỬI LOGIN XUỐNG C#
    // =====================================================
    if (btnLogin) {
        btnLogin.addEventListener("click", () => {
            const userName = document.getElementById("login-username").value || "";
            const password = document.getElementById("login-password").value || "";

            const payload = {
                action: "login",
                data: { userName, password }
            };

            if (window.chrome && window.chrome.webview) {
                window.chrome.webview.postMessage(payload);
            } else {
                // demo khi chạy index.html trực tiếp
                App.onLoginResult({
                    success: true,
                    userId: 1,
                    userName: "demo",
                    fullName: "Demo Admin",
                    roleName: "Admin",
                    roleId: 5,
                    email: "demo@example.com",
                    phoneNumber: ""
                });
            }
        });
    }
    // ==== Quên mật khẩu ở màn login ====
    const btnForgot = document.getElementById("btn-forgot-password");
    const fpModal1 = document.getElementById("modal-forgot-step1");
    const fpModal2 = document.getElementById("modal-forgot-step2");
    const fpIdentifier = document.getElementById("fp-identifier");
    const fpSendBtn = document.getElementById("btn-fp-send-code");
    const fpCodeInput = document.getElementById("fp-code");
    const fpNewPwd = document.getElementById("fp-new-password");
    const fpNewPwd2 = document.getElementById("fp-new-password2");
    const fpResetBtn = document.getElementById("btn-fp-reset");
    let fpCurrentIdentifier = "";

    if (btnForgot && fpModal1) {
        btnForgot.addEventListener("click", () => {
            fpCurrentIdentifier = "";
            if (fpIdentifier) fpIdentifier.value = "";
            fpModal1.classList.add("open");
        });
    }

    if (fpSendBtn) {
        fpSendBtn.addEventListener("click", () => {
            const v = (fpIdentifier?.value || "").trim();
            if (!v) {
                alert("Vui lòng nhập tên đăng nhập hoặc email.");
                return;
            }

            fpCurrentIdentifier = v;

            const payload = {
                action: "passwordReset_request_login",
                data: { userName: v }
            };

            if (window.chrome && window.chrome.webview) {
                window.chrome.webview.postMessage(payload);
            } else {
                alert("DEMO: gửi mã quên mật khẩu cho " + v);
                fpModal1.classList.remove("open");
                fpModal2.classList.add("open");
            }
        });
    }

    if (fpResetBtn) {
        fpResetBtn.addEventListener("click", () => {
            const code = (fpCodeInput?.value || "").trim();
            const p1 = (fpNewPwd?.value || "").trim();
            const p2 = (fpNewPwd2?.value || "").trim();

            if (!code) { alert("Vui lòng nhập mã xác nhận."); return; }
            if (!p1) { alert("Vui lòng nhập mật khẩu mới."); return; }
            if (p1 !== p2) { alert("Mật khẩu nhập lại không khớp."); return; }

            const payload = {
                action: "passwordReset_confirm_login",
                data: {
                    userName: fpCurrentIdentifier,
                    code,
                    newPassword: p1
                }
            };

            if (window.chrome && window.chrome.webview) {
                window.chrome.webview.postMessage(payload);
            } else {
                alert("DEMO: đổi mật khẩu xong.");
                fpModal2.classList.remove("open");
            }
        });
    }

    // Callback C# -> JS cho flow login
    App.onForgotPasswordCodeSentLogin = function (result) {
        alert(result.message || (result.success ? "Đã gửi mã." : "Không gửi được mã."));

        if (result.success && fpModal1 && fpModal2) {
            fpModal1.classList.remove("open");
            fpModal2.classList.add("open");
        }
    };

    App.onForgotPasswordDoneLogin = function (result) {
        alert(result.message || (result.success ? "Đổi mật khẩu thành công." : "Đổi mật khẩu thất bại."));
        if (result.success && fpModal2) {
            fpModal2.classList.remove("open");
        }
    };
    let donThuocListAll = [];
    let phieuNhapListAll = [];
    // =====================================================
    //  QUẢN LÝ TÀI KHOẢN (ADMIN)
    // =====================================================
    App.loadUserList = function () {
        if (!(window.chrome && window.chrome.webview)) {
            console.log("Demo load user list (không có WebView2)");
            return;
        }
        const payload = { action: "getUserList" };
        window.chrome.webview.postMessage(payload);
    };

    App.onUserList = function (users) {
        App.userList = users || [];

        // render theo search + filter hiện tại
        renderUserTable();

        // giữ nguyên: dùng full list để đổ dropdown bác sĩ
        refreshBacSiDropdown();
    };
    const accountTable = document.getElementById("account-user-table");
    const btnAccAdd = document.getElementById("btn-account-add");

    // Thêm tài khoản mới
    if (btnAccAdd) {
        btnAccAdd.addEventListener("click", () => {
            App.isCreatingUser = true;
            App.selectedUser = null;

            const fields = [
                "acc-username",
                "acc-fullname",
                "acc-email",
                "acc-phone",
                "acc-role",
                "acc-password",
                "acc-password-confirm"
            ];
            fields.forEach(id => {
                const el = document.getElementById(id);
                if (el) el.value = "";
            });

            const pName = document.getElementById("acc-profile-name");
            const pRole = document.getElementById("acc-profile-role");
            const pEmail = document.getElementById("acc-profile-email");
            const pAvatar = document.getElementById("acc-profile-avatar");
            if (pName) pName.textContent = "Tài khoản mới";
            if (pRole) pRole.textContent = "";
            if (pEmail) pEmail.textContent = "";
            if (pAvatar) {
                const span = pAvatar.querySelector("span");
                if (span) span.textContent = "+";
            }

            App.setAccountFormEditable(true, null);

            if (accountTable) {
                accountTable.querySelectorAll("tbody tr").forEach(r => r.classList.remove("row-selected"));
            }
        });
    }

    // Chọn 1 tài khoản từ bảng
    if (accountTable) {
        accountTable.addEventListener("click", (e) => {
            const tr = e.target.closest("tr[data-user-id]");
            if (!tr) return;

            App.isCreatingUser = false;

            const id = parseInt(tr.dataset.userId, 10);
            const user = App.userList.find(u => u.userId === id);
            if (!user) return;

            App.selectedUser = user;

            App.updateAccountUI(user);
            App.setAccountFormEditable(true, user);

            accountTable.querySelectorAll("tbody tr").forEach(r => r.classList.remove("row-selected"));
            tr.classList.add("row-selected");
        });
    }

    // Lưu tài khoản / tạo mới
    const btnAccSave = document.getElementById("btn-account-save");
    if (btnAccSave) {
        btnAccSave.addEventListener("click", () => {
            const uNameInput = document.getElementById("acc-username");
            const fullNameInput = document.getElementById("acc-fullname");
            const emailInput = document.getElementById("acc-email");
            const phoneInput = document.getElementById("acc-phone");
            const roleSelect = document.getElementById("acc-role");
            const pwInput = document.getElementById("acc-password");
            const pw2Input = document.getElementById("acc-password-confirm");

            const userName = (uNameInput?.value || "").trim();
            const fullName = (fullNameInput?.value || "").trim();
            const email = (emailInput?.value || "").trim();
            const phoneNumber = (phoneInput?.value || "").trim();
            let roleId = parseInt(roleSelect?.value || "0", 10);
            if (Number.isNaN(roleId)) roleId = 0;

            const password = (pwInput?.value || "").trim();
            const password2 = (pw2Input?.value || "").trim();

            // =========================
            //  TẠO MỚI TÀI KHOẢN
            // =========================
            if (App.isCreatingUser || !App.selectedUser) {
                if (!userName) {
                    alert("Vui lòng nhập tên đăng nhập cho tài khoản mới.");
                    return;
                }
                if (!password) {
                    alert("Vui lòng nhập mật khẩu cho tài khoản mới.");
                    return;
                }
                if (password !== password2) {
                    alert("Mật khẩu xác nhận không khớp.");
                    return;
                }

                const payload = {
                    action: "createUser",
                    data: {
                        userName,
                        fullName,
                        email,
                        phoneNumber,
                        roleId,
                        password
                    }
                };

                if (window.chrome && window.chrome.webview) {
                    window.chrome.webview.postMessage(payload);

                    // clear mật khẩu sau khi gửi
                    if (pwInput) pwInput.value = "";
                    if (pw2Input) pw2Input.value = "";

                    alert("Đã tạo tài khoản mới thành công.");
                } else {
                    console.log("Demo createUser payload:", payload);
                    alert("Demo: tạo tài khoản mới");
                }

                return;
            }

            // =========================
            //  CẬP NHẬT TÀI KHOẢN TỒN TẠI
            // =========================
            if (password && password !== password2) {
                alert("Mật khẩu xác nhận không khớp.");
                return;
            }

            const payload = {
                action: "updateUser",
                data: {
                    userId: App.selectedUser.userId,
                    fullName,
                    email,
                    phoneNumber,
                    roleId,
                    newPassword: password  // rỗng = không đổi mật khẩu
                }
            };

            if (window.chrome && window.chrome.webview) {
                window.chrome.webview.postMessage(payload);

                // clear mật khẩu sau khi gửi
                if (pwInput) pwInput.value = "";
                if (pw2Input) pw2Input.value = "";

                alert("Đã lưu thay đổi tài khoản.");
            } else {
                console.log("Demo updateUser payload:", payload);
                alert("Demo: cập nhật tài khoản");
            }
        });
    }


    // Vô hiệu hóa / kích hoạt
    const btnAccDisable = document.getElementById("btn-account-disable");
    if (btnAccDisable) {
        btnAccDisable.addEventListener("click", () => {
            if (!App.selectedUser) return;
            if (!confirm("Bạn có chắc muốn vô hiệu hóa tài khoản này?")) return;

            if (window.chrome && window.chrome.webview) {
                const payload = {
                    action: "setUserActive",
                    data: { userId: App.selectedUser.userId, isActive: false }
                };
                window.chrome.webview.postMessage(payload);
            }
        });
    }

    const btnAccActivate = document.getElementById("btn-account-activate");
    if (btnAccActivate) {
        btnAccActivate.addEventListener("click", () => {
            if (!App.selectedUser) return;

            if (window.chrome && window.chrome.webview) {
                const payload = {
                    action: "setUserActive",
                    data: { userId: App.selectedUser.userId, isActive: true }
                };
                window.chrome.webview.postMessage(payload);
            }
        });
    }

    // Xóa tài khoản
    const btnAccDelete = document.getElementById("btn-account-delete");
    if (btnAccDelete) {
        btnAccDelete.addEventListener("click", () => {
            if (!App.selectedUser) return;

            if (App.currentUser && App.currentUser.userId === App.selectedUser.userId) {
                alert("Không thể tự xóa chính tài khoản đang đăng nhập.");
                return;
            }

            if (!confirm("Bạn có chắc chắn muốn xóa tài khoản này?")) return;

            if (window.chrome && window.chrome.webview) {
                const payload = {
                    action: "deleteUser",
                    data: { userId: App.selectedUser.userId }
                };
                window.chrome.webview.postMessage(payload);
            }
        });
    }

    // =====================================================
    //  HELPER: CLEAR INPUTS TRONG 1 MODAL
    // =====================================================
    function clearInputs(container) {
        if (!container) return;

        container.querySelectorAll("input").forEach(input => {
            if (!input.disabled) input.value = "";
        });

        container.querySelectorAll("textarea").forEach(t => t.value = "");
        container.querySelectorAll("select").forEach(s => {
            if (s.options.length > 0) s.selectedIndex = 0;
        });
    }

    // =====================================================
    //  THUỐC ĐÔNG Y (UI)
    // =====================================================
    const thuocModal = document.getElementById("modal-add-thuoc");
    const thuocModalTitle = document.getElementById("modal-thuoc-title");
    const thuocIdInput = document.getElementById("thuoc-id");
    const thuocMaInput = document.getElementById("thuoc-ma");
    const thuocTenInput = document.getElementById("thuoc-ten");
    const thuocTenKhacInp = document.getElementById("thuoc-tenkhac");
    const thuocDonViSel = document.getElementById("thuoc-donvi");
    const thuocGiaInput = document.getElementById("thuoc-giaban");
    const thuocTonInput = document.getElementById("thuoc-ton");
    const thuocCongDung = document.getElementById("thuoc-congdung");
    const thuocChongCD = document.getElementById("thuoc-chongchidinh");
    const thuocGhiChu = document.getElementById("thuoc-ghichu");
    const addThuocBtn = document.querySelector('button[data-modal-target="modal-add-thuoc"]');

    if (addThuocBtn && thuocModal && thuocModalTitle) {
        addThuocBtn.addEventListener("click", () => {
            thuocModalTitle.textContent = "Thêm thuốc Đông y";

            // clear form
            if (thuocIdInput) thuocIdInput.value = "";
            [thuocMaInput, thuocTenInput, thuocTenKhacInp,
                thuocGiaInput, thuocTonInput,
                thuocCongDung, thuocChongCD, thuocGhiChu].forEach(el => {
                    if (el) el.value = "";
                });
            if (thuocDonViSel) thuocDonViSel.value = "gram";

            thuocModal.classList.add("open");
        });
    }


    document.querySelectorAll("#page-thuoc .btn-edit-thuoc").forEach(btn => {
        btn.addEventListener("click", () => {
            if (!thuocModal || !thuocModalTitle) return;

            const row = btn.closest("tr");
            if (!row) return;

            const cells = row.querySelectorAll("td");

            // 0=Mã, 1=Tên, 2=Đơn vị, 3=Giá, 4=Tồn, 5=Ghi chú
            const ma = cells[0].textContent.trim();
            const ten = cells[1].textContent.trim();
            const donvi = cells[2].textContent.trim();
            const gia = cells[3].textContent.replace(/[^\d]/g, ""); // bỏ " đ"
            const ton = cells[4].textContent.trim();
            const note = cells[5].textContent.trim();
            const tenKhac = row.dataset.tenKhac || "";
            const congDung = row.dataset.congDung || "";
            const chongChiDinh = row.dataset.chongChiDinh || "";
            // nếu khi render có set data-thuoc-id trên <tr>
            const id = row.dataset.thuocId || "";
            const tenKhacInp = document.getElementById("thuoc-tenkhac");
            const congDungInp = document.getElementById("thuoc-congdung");
            const chongChiDinhInp = document.getElementById("thuoc-chongchidinh");

            if (tenKhacInp) tenKhacInp.value = tenKhac;
            if (congDungInp) congDungInp.value = congDung;
            if (chongChiDinhInp) chongChiDinhInp.value = chongChiDinh;
            document.getElementById("thuoc-id").value = id;
            document.getElementById("thuoc-ma").value = ma;
            document.getElementById("thuoc-ten").value = ten;
            document.getElementById("thuoc-donvi").value = donvi;
            document.getElementById("thuoc-giaban").value = gia;
            document.getElementById("thuoc-ton").value = ton;
            document.getElementById("thuoc-ghichu").value = note;

            thuocModalTitle.textContent = "Sửa thuốc Đông y";
            thuocModal.classList.add("open");
        });
    });


    // =====================================================
    //  KHÁCH HÀNG (UI)
    // =====================================================
    const khachModal = document.getElementById("modal-add-khach");
    const khachModalTitle = document.getElementById("modal-khach-title");
    const addKhachBtn = document.querySelector('button[data-modal-target="modal-add-khach"]');

    if (addKhachBtn && khachModal && khachModalTitle) {
        addKhachBtn.addEventListener("click", () => {
            khachModalTitle.textContent = "Thêm khách hàng";

            const idInput = khachModal.querySelector("#khach-id");
            if (idInput) idInput.value = "0";

            clearInputs(khachModal);           // hàm bạn đã có
            khachModal.classList.add("open");  // 👈 mở modal
        });
    }

    document.querySelectorAll(".btn-edit-khach").forEach(btn => {
        btn.addEventListener("click", () => {
            if (!khachModal || !khachModalTitle) return;
            khachModalTitle.textContent = "Sửa khách hàng";
            khachModal.classList.add("open");
        });
    });

    // =====================================================
    //  XÓA THUỐC / KHÁCH (MODAL CONFIRM)
    // =====================================================
    const confirmModal = document.getElementById("modal-confirm-delete");
    const confirmMessage = document.getElementById("confirm-delete-message");
    const confirmBtn = document.getElementById("btn-confirm-delete");
    let pendingDeleteRow = null;
    let pendingDeleteType = null;   // "thuoc" hoặc "khach"
    let pendingDeleteId = 0;        // id tương ứng



    document.querySelectorAll(".btn-delete-khach").forEach(btn => {
        btn.addEventListener("click", () => {
            const row = btn.closest("tr");
            pendingDeleteRow = row;

            const code = row?.children[0]?.textContent.trim() || "";
            const name = row?.children[1]?.textContent.trim() || "";

            if (confirmMessage) {
                confirmMessage.textContent = `Bạn có chắc chắn muốn xóa khách hàng "${code} - ${name}"? (demo)`;
            }
            if (confirmModal) confirmModal.classList.add("open");
        });
    });

    if (confirmBtn) {
        confirmBtn.addEventListener("click", () => {
            if (pendingDeleteType === "thuoc" && pendingDeleteId > 0) {
                const payload = {
                    action: "deleteThuoc",
                    data: {
                        thuocId: pendingDeleteId
                    }
                };

                if (window.chrome && window.chrome.webview) {
                    window.chrome.webview.postMessage(payload);
                }
            } else if (pendingDeleteType === "khach") {
                // Sau này nếu bạn làm xóa khách hàng thì xử lý ở đây
            }

            // Có thể KHÔNG cần tự .remove() dòng vì C# sẽ load lại list
            // nếu bạn vẫn muốn thấy hiệu ứng ngay lập tức thì có thể giữ:
            // if (pendingDeleteRow) pendingDeleteRow.remove();

            pendingDeleteRow = null;
            pendingDeleteType = null;
            pendingDeleteId = 0;

            if (confirmModal) confirmModal.classList.remove("open");
        });
    }


    // =====================================================
    //  THÊM / XOÁ DÒNG CHI TIẾT (ĐƠN THUỐC & NHẬP KHO)
    // =====================================================
    const addRowButtons = document.querySelectorAll(".btn-add-row");

    addRowButtons.forEach(btn => {
        btn.addEventListener("click", (e) => {
            e.preventDefault();
            const tableId = btn.getAttribute("data-table");
            const table = document.getElementById(tableId);
            if (!table) return;

            const tbody = table.querySelector("tbody");
            if (!tbody || !tbody.rows.length) return;

            const templateRow = tbody.rows[tbody.rows.length - 1];
            const newRow = templateRow.cloneNode(true);

            newRow.querySelectorAll("input").forEach(input => {
                input.value = "";
            });

            const cells = newRow.querySelectorAll("td");
            if (cells.length >= 5) cells[4].textContent = "";

            tbody.appendChild(newRow);
        });
    });

    document.querySelectorAll(".table-compact").forEach(table => {
        table.addEventListener("click", (e) => {
            const removeBtn = e.target.closest(".btn-row-remove");
            if (!removeBtn) return;

            const row = removeBtn.closest("tr");
            const tbody = row.parentElement;

            if (tbody.rows.length > 1) {
                tbody.removeChild(row);
            }
        });
    });

    // =====================================================
    //  ĐƠN THUỐC: LẬP / SỬA / XEM / IN
    // =====================================================
    // =====================================================
    //  ĐƠN THUỐC: TẢI DANH SÁCH + XEM CHI TIẾT
    // =====================================================

    function formatDateVi(dateStr) {
        if (!dateStr) return "";
        const d = new Date(dateStr);
        if (isNaN(d.getTime())) return "";
        const dd = String(d.getDate()).padStart(2, "0");
        const mm = String(d.getMonth() + 1).padStart(2, "0");
        const yyyy = d.getFullYear();
        return `${dd}/${mm}/${yyyy}`;
    }

    function formatMoneyVnd(v) {
        if (v == null || isNaN(v)) v = 0;
        return v.toLocaleString("vi-VN") + " đ";
    }

    function mapTrangThaiDon(code) {
        switch (code) {
            case 0: return { text: "Nháp", css: "badge-outline" };
            case 1: return { text: "Đã kê đơn", css: "badge-outline" };
            case 2: return { text: "Chưa lấy thuốc", css: "badge-warning" };
            case 3: return { text: "Đã phát thuốc", css: "badge-success" };
            case 4: return { text: "Đã hủy", css: "badge-outline" };
            default: return { text: "", css: "badge-outline" };
        }
    }

    function mapTrangThaiThanhToan(code) {
        switch (code) {
            case 0: return { text: "Chưa thanh toán", css: "badge-outline" };
            case 1: return { text: "Trả một phần", css: "badge-warning" };
            case 2: return { text: "Đã thanh toán", css: "badge-success" };
            case 3: return { text: "Không thu", css: "badge-outline" };
            default: return { text: "", css: "badge-outline" };
        }
    }

    function requestDonThuocList() {
        const payload = { action: "getDonThuocList", data: {} };

        if (window.chrome && window.chrome.webview) {
            window.chrome.webview.postMessage(payload);
        } else {
            // demo khi mở file tĩnh
            const demo = [{
                donThuocId: 1,
                soDon: "DT2025-0005",
                ngayLap: "2025-11-27T00:00:00",
                khachHangId: 5,
                maKhachHang: "KH005",
                tenKhachHang: "Lê Văn E",
                bacSiKeDon: "Thầy Hùng",
                tongTienHang: 700000,
                giamGia: 0,
                tienKhachPhaiTra: 700000,
                daThanhToan: 500000,
                conNo: 200000,
                hanThanhToan: "2025-11-30T00:00:00",
                trangThaiDon: 3,
                trangThaiThanhToan: 1
            }];
            App.onDonThuocList(demo);
        }
    }
    // Đặt ở đầu file (sau khi đã có window.App = window.App || {} )
    window.App = window.App || {};

    let thuocListForDonThuoc = [];
    let thuocListLoadedForDonThuoc = false;
    let donThuocPendingAddRow = false;
    let nhapKhoPendingAddRow = false;    
    let thuocListAll = [];
    App.onThuocList = function (items) {
        // dùng cho Đơn thuốc
        thuocListForDonThuoc = items || [];
        thuocListLoadedForDonThuoc = true;

        // dùng cho search danh mục
        thuocListAll = items || [];
        renderThuocTable();
    };

    function requestDonThuocDetail(donThuocId) {
        const payload = {
            action: "getDonThuocDetail",
            data: { donThuocId }
        };

        if (window.chrome && window.chrome.webview) {
            window.chrome.webview.postMessage(payload);
        } else {
            console.log("DEMO getDonThuocDetail", payload);
        }
    }

    App.onDonThuocList = function (items) {
        // lưu lại full list, lần đầu load
        donThuocListAll = items || [];
        // render theo filter hiện tại (nếu user đã chọn ngày / gõ text)
        renderDonThuocTable();
    };
    function getThuocSearchText() {
        const input = document.getElementById("thuoc-search");
        return (input?.value || "").trim().toLowerCase();
    }

    function filterThuocList(list) {
        const keyword = getThuocSearchText();
        if (!keyword) return list || [];

        return (list || []).filter(t => {
            const ten = (t.tenThuoc || "").toLowerCase();
            const ma = (t.maThuoc || "").toLowerCase();
            const tenKhac = (t.tenKhac || "").toLowerCase();
            return ten.includes(keyword) || ma.includes(keyword) || tenKhac.includes(keyword);
        });
    }

    function renderThuocTable() {
        const tbody = document.querySelector("#page-thuoc table tbody");
        if (!tbody) return;

        const items = filterThuocList(thuocListAll);

        tbody.innerHTML = "";

        if (!items || !items.length) {
            const tr = document.createElement("tr");
            tr.innerHTML = "<td colspan='7'>Chưa có dữ liệu thuốc.</td>";
            tbody.appendChild(tr);
            if (typeof bindThuocRowEvents === "function") {
                bindThuocRowEvents();
            }
            return;
        }

        // sort như code cũ
        items.sort((a, b) => {
            const maA = (a.maThuoc || "").toString();
            const maB = (b.maThuoc || "").toString();
            return maA.localeCompare(maB, "vi-VN", {
                numeric: true,
                sensitivity: "base"
            });
        });

        items.forEach(t => {
            const tr = document.createElement("tr");

            tr.dataset.thuocId = t.thuocId;
            tr.dataset.tenKhac = t.tenKhac || "";
            tr.dataset.congDung = t.congDung || "";
            tr.dataset.chongChiDinh = t.chongChiDinh || "";
            tr.dataset.soLuongTon = t.soLuongTon || 0;
            tr.dataset.tonToiThieu = t.tonToiThieu || 0;
            tr.dataset.ghiChu = t.ghiChu || "";

            tr.innerHTML = `
            <td>${t.maThuoc}</td>
            <td>${t.tenThuoc}</td>
            <td>${t.donViTinh}</td>
            <td>${(t.giaBanLe || 0).toLocaleString("vi-VN")} đ</td>
            <td>${t.soLuongTon || 0}</td> 
            <td>${t.tonToiThieu || 0}</td> 
            <td>${t.ghiChu || ""}</td>
            <td>
                <button class="btn btn-sm btn-edit-thuoc" type="button">Sửa</button>
                <button class="btn btn-sm btn-delete-thuoc" type="button">Xóa</button>
            </td>`;

            tbody.appendChild(tr);

            // logic cũ giữ nguyên
            if (donThuocPendingAddRow) {
                donThuocPendingAddRow = false;
                addDonThuocRow();
            }
            if (nhapKhoPendingAddRow) {
                nhapKhoPendingAddRow = false;
                addNhapKhoRow();
            }
        });

        if (typeof bindThuocRowEvents === "function") {
            bindThuocRowEvents();
        }
    }

    App.onCompleteDonThuocResult = function (res) {
        if (!res) return;

        if (res.message) {
            alert(res.message);
        }

        if (res.success) {
            // 🔥 load lại danh sách đơn để thấy trạng thái mới
            if (typeof App.loadDonThuocList === "function") {
                App.loadDonThuocList();
            } else if (typeof App.reloadDonThuocList === "function") {
                App.reloadDonThuocList();
            } else {
                // fallback: tự gọi API đơn giản
                if (window.chrome && window.chrome.webview) {
                    window.chrome.webview.postMessage({
                        action: "getDonThuocList",
                        data: {} // hoặc gửi filter đang chọn nếu bạn có lưu
                    });
                }
            }
        }
    };
    function getDonThuocFilterValues() {
        const from = document.getElementById("dt-filter-from")?.value || "";
        const to = document.getElementById("dt-filter-to")?.value || "";
        const khach = (document.getElementById("dt-filter-khach")?.value || "").trim().toLowerCase();
        const keyword = (document.getElementById("dt-filter-keyword")?.value || "").trim().toLowerCase();

        return { from, to, khach, keyword };
    }

    function filterDonThuocList(list) {
        const { from, to, khach, keyword } = getDonThuocFilterValues();

        return (list || []).filter(dt => {
            const ngayLapStr = dt.ngayLap ? dt.ngayLap.substring(0, 10) : "";

            if (from && ngayLapStr && ngayLapStr < from) return false;
            if (to && ngayLapStr && ngayLapStr > to) return false;

            if (khach) {
                const ma = (dt.maKhachHang || "").toLowerCase();
                const ten = (dt.tenKhachHang || "").toLowerCase();
                if (!ma.includes(khach) && !ten.includes(khach)) return false;
            }

            if (keyword) {
                const soDon = (dt.soDon || "").toLowerCase();
                const ghiChu = (dt.ghiChu || "").toLowerCase();
                if (!soDon.includes(keyword) && !ghiChu.includes(keyword)) return false;
            }

            return true;
        });
    }

    function renderDonThuocTable() {
        const tbody = document.querySelector("#page-donthuoc table tbody");
        if (!tbody) return;

        const items = filterDonThuocList(donThuocListAll);

        tbody.innerHTML = "";

        if (!items || !items.length) {
            const tr = document.createElement("tr");
            tr.innerHTML = "<td colspan='9'>Chưa có dữ liệu đơn thuốc.</td>";
            tbody.appendChild(tr);
            bindDonThuocRowEvents();
            return;
        }

        items.forEach(dt => {
            const tr = document.createElement("tr");
            tr.dataset.donThuocId = dt.donThuocId || 0;

            const soDon = dt.soDon || "";
            const ngayLap = formatDateVi(dt.ngayLap);
            const khachText =
                (dt.maKhachHang ? dt.maKhachHang + " - " : "") +
                (dt.tenKhachHang || "");
            const bacSi = dt.bacSiKeDon || "";
            const tongTien = (dt.tienKhachPhaiTra != null
                ? dt.tienKhachPhaiTra
                : (dt.tongTienHang || 0));
            const tongTienText = formatMoneyVnd(tongTien);
            const hanTT = dt.hanThanhToan ? formatDateVi(dt.hanThanhToan) : "";

            const statusDon = mapTrangThaiDon(dt.trangThaiDon);
            const statusTt = mapTrangThaiThanhToan(dt.trangThaiThanhToan);

            const canComplete = dt.trangThaiDon === 0;

            const canEditDelete = dt.trangThaiDon === 0 && !isThuNgan();

            tr.innerHTML = `
            <td>${soDon}</td>
            <td>${ngayLap}</td>
            <td>${khachText}</td>
            <td>${bacSi}</td>
            <td>${tongTienText}</td>
            <td>${hanTT}</td>
            <td><span class="badge ${statusDon.css}">${statusDon.text}</span></td>
            <td><span class="badge ${statusTt.css}">${statusTt.text}</span></td>
            <td>
                <button class="btn btn-sm btn-view-donthuoc">Xem</button>

                ${canEditDelete
                            ? '<button class="btn btn-sm btn-edit-donthuoc">Sửa</button>'
                            : ''}

                ${canComplete
                            ? '<button class="btn btn-sm btn-success btn-complete-donthuoc">Hoàn tất</button>'
                            : ''}

                <button class="btn btn-sm btn-print-donthuoc">In</button>

                <button class="btn btn-sm btn-pay-donthuoc">Thu tiền</button>

                ${canEditDelete
                            ? '<button class="btn btn-sm btn-danger btn-delete-donthuoc">Xóa</button>'
                            : ''}
            </td>
        `;

            tbody.appendChild(tr);
        });

        bindDonThuocRowEvents();
    }

    function getPhieuNhapFilterValues() {
        const from = document.getElementById("pn-filter-from")?.value || "";
        const to = document.getElementById("pn-filter-to")?.value || "";
        const ncc = (document.getElementById("pn-filter-ncc-name")?.value || "").trim().toLowerCase();
        const soPhieu = (document.getElementById("pn-filter-sophieu")?.value || "").trim().toLowerCase();

        return { from, to, ncc, soPhieu };
    }

    function filterPhieuNhapList(list) {
        const { from, to, ncc, soPhieu } = getPhieuNhapFilterValues();

        return (list || []).filter(pn => {
            const ngayNhapStr = pn.ngayNhap ? pn.ngayNhap.substring(0, 10) : "";

            if (from && ngayNhapStr && ngayNhapStr < from) return false;
            if (to && ngayNhapStr && ngayNhapStr > to) return false;

            if (ncc) {
                const ma = (pn.maNcc || "").toLowerCase();
                const ten = (pn.tenNcc || "").toLowerCase();
                const combine = `${ma} - ${ten}`.trim();
                if (!ma.includes(ncc) && !ten.includes(ncc) && !combine.includes(ncc)) return false;
            }

            if (soPhieu) {
                const sp = (pn.soPhieu || "").toLowerCase();
                if (!sp.includes(soPhieu)) return false;
            }

            return true;
        });
    }

    function renderPhieuNhapTable() {
        const tbody = document.querySelector("#page-nhapkho table tbody");
        if (!tbody) return;

        const items = filterPhieuNhapList(phieuNhapListAll);

        tbody.innerHTML = "";

        if (!items || !items.length) {
            const tr = document.createElement("tr");
            tr.innerHTML = "<td colspan='9'>Chưa có dữ liệu phiếu nhập.</td>";
            tbody.appendChild(tr);
            return;
        }

        items.forEach(pn => {
            const tr = document.createElement("tr");
            tr.dataset.phieuNhapId = pn.phieuNhapId;
            tr.dataset.nhaCungCapId = pn.nhaCungCapId;
            tr.dataset.tenNcc = pn.tenNcc || "";
            tr.dataset.tienPhaiTra = pn.tienPhaiTra ?? pn.tongTienHang ?? 0;
            tr.dataset.daThanhToan = pn.daThanhToan ?? 0;
            tr.dataset.conNo = pn.conNo ?? 0;
            tr.dataset.trangThaiTt = pn.trangThaiThanhToan ?? 0;

            const soPhieu = pn.soPhieu || "";
            const ngayNhap = formatDateVi(pn.ngayNhap);
            const nccText = (pn.maNcc ? pn.maNcc + " - " : "") + (pn.tenNcc || "");
            const soMatHang = pn.soMatHang || 0;
            const tongSoLuong = (pn.tongSoLuong || 0).toLocaleString("vi-VN") + " gram";
            const tongTienText = formatMoneyVnd(pn.tienPhaiTra ?? pn.tongTienHang);
            const hanTT = pn.hanThanhToan ? formatDateVi(pn.hanThanhToan) : "";

            let ttBadge = "";
            const conNo = pn.conNo || 0;

            switch (pn.trangThaiThanhToan) {
                case 0:
                    ttBadge = `<span class="badge badge-outline">Chưa thanh toán</span>`;
                    break;
                case 1:
                    ttBadge = `<span class="badge badge-success">Đã thanh toán</span>`;
                    break;
                case 2:
                    ttBadge = `<span class="badge badge-warning">Còn nợ ${formatMoneyVnd(conNo)}</span>`;
                    break;
                default:
                    ttBadge = `<span class="badge badge-outline">Không rõ</span>`;
            }

            const showPayBtn = pn.conNo > 0;

            tr.innerHTML = `
            <td>${soPhieu}</td>
            <td>${ngayNhap}</td>
            <td>${nccText}</td>
            <td>${soMatHang}</td>
            <td>${tongSoLuong}</td>
            <td>${tongTienText}</td>
            <td>${hanTT}</td>
            <td>${ttBadge}</td>
            <td>
                <button class="btn btn-sm btn-view-nhapkho" type="button">Xem</button>
                <button class="btn btn-sm btn-print-nhapkho" type="button">In</button>
                ${showPayBtn
                    ? '<button class="btn btn-sm btn-primary btn-thanh-toan-ncc" type="button">Thanh toán NCC</button>'
                    : ''
                }
            </td>
        `;

            tbody.appendChild(tr);
        });

        bindNhapKhoRowEvents();
    }

    App.onDonThuocDetail = function (detail) {
        if (!detail) return;

        const modal = document.getElementById("modal-view-donthuoc");
        if (!modal) return;

        const khachText =
            (detail.maKhachHang ? detail.maKhachHang + " - " : "") +
            (detail.tenKhachHang || "");

        const el = id => document.getElementById(id);

        if (el("view-dt-sodon")) el("view-dt-sodon").textContent = detail.soDon || "";
        if (el("view-dt-khach")) el("view-dt-khach").textContent = khachText;
        if (el("view-dt-namsinh")) el("view-dt-namsinh").textContent = detail.namSinh || "";
        if (el("view-dt-diachi")) el("view-dt-diachi").textContent = detail.diaChi || "";
        if (el("view-dt-ngaylap")) el("view-dt-ngaylap").textContent = formatDateVi(detail.ngayLap);
        if (el("view-dt-bacsi")) el("view-dt-bacsi").textContent = detail.bacSiKeDon || "";
        if (el("view-dt-hanthanhtoan"))
            el("view-dt-hanthanhtoan").textContent =
                detail.hanThanhToan ? formatDateVi(detail.hanThanhToan) : "";

        const stDon = mapTrangThaiDon(detail.trangThaiDon);
        const stTt = mapTrangThaiThanhToan(detail.trangThaiThanhToan);

        const badgeDon = el("view-dt-trangthaidon-badge");
        if (badgeDon) {
            badgeDon.textContent = stDon.text;
            badgeDon.className = "badge " + stDon.css;
        }

        const badgeTt = el("view-dt-trangthaithanhtoan-badge");
        if (badgeTt) {
            badgeTt.textContent = stTt.text;
            badgeTt.className = "badge " + stTt.css;
        }

        if (el("view-dt-chandoan"))
            el("view-dt-chandoan").textContent =
                detail.chanDoan || detail.ghiChu || "";

        const tbody = el("view-dt-ct-body");
        if (tbody) {
            tbody.innerHTML = "";

            if (detail.chiTiet && detail.chiTiet.length) {
                detail.chiTiet.forEach((line, idx) => {
                    const tr = document.createElement("tr");
                    tr.innerHTML = `
                        <td>${idx + 1}</td>
                        <td>${line.tenThuoc || ""}</td>
                        <td>${line.lieuLuongGram ?? ""}</td>
                        <td>${line.soThang ?? ""}</td>
                        <td>${formatMoneyVnd(line.donGiaBan ?? 0)}</td>
                        <td>${formatMoneyVnd(line.thanhTien ?? 0)}</td>
                    `;
                    tbody.appendChild(tr);
                });
            } else {
                const tr = document.createElement("tr");
                tr.innerHTML = "<td colspan='6'>Không có chi tiết.</td>";
                tbody.appendChild(tr);
            }
        }

        if (el("view-dt-tongtien"))
            el("view-dt-tongtien").textContent = formatMoneyVnd(detail.tongTienHang);
        if (el("view-dt-giamgia"))
            el("view-dt-giamgia").textContent = formatMoneyVnd(detail.giamGia);
        if (el("view-dt-phaitra"))
            el("view-dt-phaitra").textContent = formatMoneyVnd(
                detail.tienKhachPhaiTra != null
                    ? detail.tienKhachPhaiTra
                    : (detail.tongTienHang - detail.giamGia)
            );
        if (el("view-dt-dathanhtoan"))
            el("view-dt-dathanhtoan").textContent = formatMoneyVnd(detail.daThanhToan);
        if (el("view-dt-conno"))
            el("view-dt-conno").textContent = formatMoneyVnd(detail.conNo);

        modal.classList.add("open");
    };
    // ===============================
    //  PHÂN QUYỀN FORM ĐƠN THUỐC
    // ===============================
    function applyDonThuocRolePermissions(isEdit) {
        const u = (window.App && App.currentUser) ? App.currentUser : null;
        const roleName = (u && u.roleName ? u.roleName : "").toLowerCase();
        const isAdmin = roleName === "admin";

        const $bacSi = $("#dt-bacsi");
        const $giamGia = $("#dt-giamgia");

        // ----- GIẢM GIÁ -----
        // Chỉ Admin mới được nhập giảm giá
        if (isAdmin && !isEdit) {
            $giamGia.prop("disabled", false);
        } else {
            // Khi sửa đơn hoặc không phải admin -> khóa
            $giamGia.prop("disabled", true);
        }

        // ----- BÁC SĨ KÊ ĐƠN -----
        if (!u) {
            // Không có user thì cứ cho sửa tự do (trường hợp demo file tĩnh)
            $bacSi.prop("disabled", false);
            return;
        }

        if (isEdit) {
            // Đang SỬA đơn -> KHÓA luôn bác sĩ kê đơn cho mọi role
            $bacSi.prop("disabled", true);
            return;
        }

        // Đang TẠO đơn mới
        if (isAdmin) {
            // Admin: được chọn / gõ bác sĩ (sau này bạn thay bằng <select> cũng được)
            $bacSi.prop("disabled", false);
            // không auto fill tên
        } else {
            // Các role khác: tự động gán tên họ, KHÔNG cho sửa
            const ten = u.fullName || u.userName || "";
            $bacSi.val(ten);
            $bacSi.prop("disabled", true);
        }
    }

    function bindDonThuocRowEvents() {
        const modalDonThuocEdit = document.getElementById("modal-add-donthuoc");
        const modalDonThuocTitle = document.getElementById("modal-donthuoc-title");

        // ====== NÚT "LẬP ĐƠN THUỐC MỚI" ======
        const btnAddDonThuoc = document.querySelector(
            'button[data-modal-target="modal-add-donthuoc"]'
        );

        if (btnAddDonThuoc && modalDonThuocEdit && modalDonThuocTitle) {
            btnAddDonThuoc.onclick = () => {
                modalDonThuocTitle.textContent = "Lập đơn thuốc mới";

                if (!khachListLoadedForDonThuoc &&
                    window.chrome && window.chrome.webview) {
                    window.chrome.webview.postMessage({
                        action: "getKhachList",
                        data: {}
                    });
                }

                if (!thuocListLoadedForDonThuoc &&
                    window.chrome && window.chrome.webview) {
                    window.chrome.webview.postMessage({
                        action: "getThuocList",
                        data: {}
                    });
                }

                if (window.$) {
                    resetDonThuocFormForCreate();
                }

                modalDonThuocEdit.classList.add("open");
            };
        }

        // ====== SỬA ĐƠN THUỐC ======
        document
            .querySelectorAll("#page-donthuoc .btn-edit-donthuoc")
            .forEach(btn => {
                btn.onclick = () => {
                    const row = btn.closest("tr");
                    if (!row) return;

                    const id = parseInt(row.dataset.donThuocId || "0", 10);
                    if (!id) return;

                    if (window.chrome && window.chrome.webview) {
                        window.chrome.webview.postMessage({
                            action: "loadDonThuocForEdit",
                            data: { DonThuocId: id }
                        });
                    } else {
                        console.log("DEMO loadDonThuocForEdit", id);
                    }
                };
            });

        // ====== XEM ĐƠN THUỐC ======
        document
            .querySelectorAll("#page-donthuoc .btn-view-donthuoc")
            .forEach(btn => {
                btn.onclick = () => {
                    const row = btn.closest("tr");
                    if (!row) return;

                    const id = parseInt(row.dataset.donThuocId || "0", 10);
                    if (!id) return;

                    requestDonThuocDetail(id); // hàm bạn đã có sẵn
                };
            });

        // ====== IN ĐƠN THUỐC ======
        document
            .querySelectorAll("#page-donthuoc .btn-print-donthuoc")
            .forEach(btn => {
                btn.onclick = () => {
                    const row = btn.closest("tr");
                    if (!row) return;

                    const id = parseInt(row.dataset.donThuocId || "0", 10);
                    if (!id) return;

                    if (window.chrome && window.chrome.webview) {
                        window.chrome.webview.postMessage({
                            action: "printDonThuoc",
                            data: { DonThuocId: id }
                        });
                    } else {
                        alert("In đơn thuốc (demo)");
                    }
                };
            });

        // ====== HOÀN TẤT ĐƠN (NHÁP -> ĐÃ KÊ ĐƠN) ======
        document
            .querySelectorAll("#page-donthuoc .btn-complete-donthuoc")
            .forEach(btn => {
                btn.onclick = () => {
                    const tr = btn.closest("tr");
                    if (!tr) return;

                    const id = parseInt(tr.dataset.donThuocId || "0", 10);
                    if (!id) return;

                    if (!confirm("Xác nhận chuyển đơn này từ Nháp sang Đã kê đơn?")) return;

                    if (window.chrome && window.chrome.webview) {
                        window.chrome.webview.postMessage({
                            action: "completeDonThuoc",
                            data: { DonThuocId: id }
                        });
                    }
                };
            });
        // ====== XÓA ĐƠN THUỐC ======
        document
            .querySelectorAll("#page-donthuoc .btn-delete-donthuoc")
            .forEach(btn => {
                btn.onclick = () => {
                    const tr = btn.closest("tr");
                    if (!tr) return;

                    const id = parseInt(tr.dataset.donThuocId || "0", 10);
                    if (!id) return;

                    if (!confirm("Bạn có chắc muốn xóa đơn thuốc này?")) return;

                    if (window.chrome && window.chrome.webview) {
                        window.chrome.webview.postMessage({
                            action: "deleteDonThuoc",
                            data: { DonThuocId: id }
                        });
                    }
                };
            });
        // ====== THU TIỀN ĐƠN THUỐC (MỞ MODAL) ======
        document
            .querySelectorAll("#page-donthuoc .btn-pay-donthuoc")
            .forEach(btn => {
                btn.onclick = () => {
                    const tr = btn.closest("tr");
                    if (!tr) return;

                    const id = parseInt(tr.dataset.donThuocId || "0", 10);
                    if (!id) return;

                    // Gửi sang C# xin chi tiết để fill modal
                    if (window.chrome && window.chrome.webview) {
                        window.chrome.webview.postMessage({
                            action: "getDonThuocForPayment",
                            data: { DonThuocId: id }
                        });
                    }
                };
            });



    }
    App.onDonThuocForPayment = function (detail) {
        if (!detail) {
            alert("Không load được thông tin đơn thuốc.");
            return;
        }

        // Lưu id đơn
        $("#phieuthu-donthuoc-id").val(detail.donThuocId);

        // Text khách + số đơn
        const khachText =
            (detail.maKhachHang ? detail.maKhachHang + " - " : "") +
            (detail.tenKhachHang || "");

        $("#phieuthu-khach").val(khachText);
        $("#phieuthu-sodon").val(detail.soDon || "");

        // tiền phải trả, đã thu, còn nợ
        const phaiTra = detail.tienKhachPhaiTra || (detail.tongTienHang - detail.giamGia);
        const daThu = detail.daThanhToan || 0;
        const conNo = detail.conNo != null ? detail.conNo : (phaiTra - daThu);

        const fm = v => (v || 0).toLocaleString("vi-VN") + " đ";

        $("#phieuthu-phaitra").val(fm(phaiTra));
        $("#phieuthu-dathu").val(fm(daThu));
        $("#phieuthu-sotien").val(fm(conNo));     // mặc định thu hết nợ
        $("#phieuthu-ghichu").val("");

        // Mặc định tiền mặt
        $("#phieuthu-hinhthuc").val("0");

        // Mở modal
        $("#modal-phieuthu").addClass("open");
    };
    $("#btn-phieuthu-save").on("click", function () {
        const donThuocId = parseInt($("#phieuthu-donthuoc-id").val() || "0", 10);
        if (!donThuocId) {
            alert("Thiếu thông tin đơn thuốc.");
            return;
        }

        // lấy số tiền: cho phép nhập 10.000 / 10,000 / 10000
        const raw = $("#phieuthu-sotien").val() || "";
        const cleaned = String(raw).replace(/[^\d]/g, "");
        const soTien = parseFloat(cleaned || "0");

        if (!soTien || soTien <= 0) {
            alert("Số tiền thu không hợp lệ.");
            return;
        }

        const hinhThuc = parseInt($("#phieuthu-hinhthuc").val() || "0", 10);
        const ghiChu = $("#phieuthu-ghichu").val() || "";

        if (window.chrome && window.chrome.webview) {
            window.chrome.webview.postMessage({
                action: "thuTienDonThuoc",
                data: {
                    DonThuocId: donThuocId,
                    SoTien: soTien,
                    HinhThucThanhToan: hinhThuc,
                    GhiChu: ghiChu
                }
            });
        }
    });

    App.onThuTienDonThuocResult = function (res) {
        if (!res) return;

        alert(res.message || (res.success ? "Thu tiền thành công." : "Thu tiền thất bại."));

        if (res.success) {
            // đóng modal
            $("#modal-phieuthu").removeClass("open");

            // reload danh sách đơn để cập nhật "Còn nợ", "Thanh toán"
            if (typeof App.loadDonThuocList === "function") {
                App.loadDonThuocList();
            } else if (window.chrome && window.chrome.webview) {
                window.chrome.webview.postMessage({
                    action: "getDonThuocList",
                    data: {}
                });
            }
        }
    };


    App.onDeleteDonThuocResult = function (res) {
        if (!res) return;

        alert(res.message || (res.success ? "Đã xóa đơn thuốc." : "Xóa đơn thuốc thất bại."));

        if (res.success) {
            if (typeof App.loadDonThuocList === "function") {
                App.loadDonThuocList();
            } else if (window.chrome && window.chrome.webview) {
                window.chrome.webview.postMessage({
                    action: "getDonThuocList",
                    data: {}
                });
            }
        }
    };


    function collectDonThuocForm() {
        const id = $("#dt-id").val() ? parseInt($("#dt-id").val(), 10) : 0;
        const isEdit = id > 0;

        const khachId = parseInt($("#dt-khachhang").val() || "0", 10) || null;

        const tongTienHang = parseFloat($("#dt-tongtienhang").data("v") || 0);
        const giamGia = parseFloat($("#dt-giamgia").val() || 0);
        const daThanhToan = parseFloat($("#dt-da-thanh-toan").data("v") || 0);

        // ✅ tiền khách phải trả = tổng - giảm (không âm)
        let tienKhachPhaiTra = tongTienHang - giamGia;
        if (tienKhachPhaiTra < 0) tienKhachPhaiTra = 0;

        // ✅ trạng thái đơn:
        // - Tạo mới: luôn là Nháp (0)
        // - Sửa: giữ lại trạng thái hiện tại (đọc từ hidden/select nếu có)
        let trangThaiDon = 0;
        const $sttDon = $("#dt-trangthai-don");
        if (isEdit && $sttDon.length) {
            trangThaiDon = parseInt($sttDon.val() || "0", 10);
        }

        return {
            DonThuocId: id,
            KhachHangId: khachId,
            NgayLap: $("#dt-ngaylap").val(),
            BacSiKeDon: $("#dt-bacsi").val() || "",
            ChanDoan: $("#dt-chandoan").val() || "",
            TongTienHang: tongTienHang,
            GiamGia: giamGia,
            TienKhachPhaiTra: tienKhachPhaiTra,
            DaThanhToan: daThanhToan,
            TrangThaiDon: trangThaiDon,          // 🔹 rất quan trọng
            GhiChu: $("#dt-ghichu").val() || ""
        };
    }

    function resetDonThuocFormForCreate() {
        // Id = 0 để backend hiểu là tạo mới
        $("#dt-id").val("0");

        // ngày lập = hôm nay (nếu bạn muốn)
        const today = new Date().toISOString().substring(0, 10);
        $("#dt-ngaylap").val(today);

        // khách hàng + chẩn đoán
        $("#dt-khachhang").val("");
        $("#dt-chan-doan").val("");
        $("#dt-chandoan").val("");
        $("#dt-ghi-chu").val("");

        // Giảm giá: luôn cho nhập, reset về 0
        $("#dt-giamgia")
            .val("0")
            .prop("disabled", false)
            .prop("readonly", false);

        // Reset tiền: tổng tiền hàng / phải trả / đã thanh toán / còn nợ
        const resetMoney = (sel) => {
            const $el = $(sel);
            $el.data("v", 0);
            $el.text("0 đ");
        };
        resetMoney("#dt-tongtienhang");
        resetMoney("#dt-phaitra");
        resetMoney("#dt-da-thanh-toan");
        resetMoney("#dt-con-no");

        // Xóa hết dòng chi tiết đơn thuốc
        $("#table-donthuoc-details tbody").empty();

        // Bác sĩ kê đơn: set lại theo role
        const roleName = (App.currentUser?.roleName || "").toLowerCase();
        const isAdmin = roleName === "admin";
        const select = document.getElementById("dt-bacsi");

        if (select && App.currentUser) {
            if (isAdmin) {
                // Admin: cho phép chọn, giữ dropdown như đã build từ user list
                select.disabled = false;
                // nếu bạn muốn khi tạo mới thì để trống:
                select.value = "";
            } else {
                // Bác sĩ: luôn là tên mình và khóa
                const name = App.currentUser.fullName || App.currentUser.userName || "";
                select.innerHTML = "";
                const opt = document.createElement("option");
                opt.value = name;
                opt.textContent = name;
                select.appendChild(opt);
                select.value = name;
                select.disabled = true;
            }
        }
    }



    // ===============================
    //  COLLECT CHI TIẾT ĐƠN THUỐC
    // ===============================
    function collectDonThuocDetails() {
        const rows = $("#table-donthuoc-details tbody tr");
        const list = [];

        rows.each(function () {
            const thuocId = parseInt($(this).find(".dt-thuoc").val());
            if (!thuocId) return;

            const lieu = parseFloat($(this).find(".dt-lieu").val()) || 0;
            const soThang = parseInt($(this).find(".dt-sothang").val()) || 0;
            const donGia = parseFloat($(this).find(".dt-dongia").val()) || 0;

            // ✅ Thành tiền = liều (gram) × số thang × đơn giá
            const thanhTien = lieu * soThang * donGia;

            list.push({
                ThuocId: thuocId,
                LieuLuongGram: lieu,
                SoThang: soThang,
                DonGiaBan: donGia,
                ThanhTien: thanhTien,
                GhiChu: ""
            });
        });

        return list;
    }


    function addDonThuocRow() {
        const $tbody = $("#table-donthuoc-details tbody");
        if (!$tbody.length) return;

        // Nếu chưa có danh sách thuốc thì không thêm
        if (!thuocListForDonThuoc.length) {
            alert("Chưa có thuốc trong danh mục. Hãy thêm thuốc trước.");
            return;
        }

        const optionsHtml = thuocListForDonThuoc.map(t => `
        <option value="${t.thuocId}" data-gia="${t.giaBanLe}">
            ${t.tenThuoc}
        </option>
    `).join("");

        const $tr = $(`
        <tr>
            <td>
                <select class="input dt-thuoc">
                    <option value="">-- Chọn thuốc --</option>
                    ${optionsHtml}
                </select>
            </td>
            <td><input class="input dt-lieu" value="0" /></td>
            <td><input class="input dt-sothang" value="1" /></td>
            <td><input class="input dt-dongia" readonly /></td>
            <td class="dt-thanhtien" data-v="0">0</td>
            <td>
                <button class="btn btn-sm btn-danger btn-row-remove">X</button>
            </td>
        </tr>
    `);

        $tbody.append($tr);

        // sự kiện trong dòng
        function recalcRow() {
            const lieu = parseFloat($tr.find(".dt-lieu").val()) || 0;
            const soThang = parseFloat($tr.find(".dt-sothang").val()) || 0;
            const donGia = parseFloat($tr.find(".dt-dongia").val()) || 0;

            // ✅ liều × số thang × đơn giá
            const tt = lieu * soThang * donGia;

            $tr.find(".dt-thanhtien")
                .data("v", tt)
                .text(tt.toLocaleString("vi-VN"));
        }
        function recalcTotals() {
            let tong = 0;
            $("#table-donthuoc-details tbody .dt-thanhtien").each(function () {
                tong += $(this).data("v") || 0;
            });

            const giamGia = parseFloat($("#dt-giamgia").val()) || 0;
            const phaiTra = tong - giamGia;
            const daThanhToan = $("#dt-da-thanh-toan").data("v") || 0;
            const conNo = phaiTra - daThanhToan;

            const setMoney = (selector, value) => {
                const $el = $(selector);
                $el.data("v", value);
                $el.text(value.toLocaleString("vi-VN") + " đ");
            };

            setMoney("#dt-tongtienhang", tong);
            setMoney("#dt-phaitra", phaiTra);
            setMoney("#dt-con-no", conNo);
        }

        // chọn thuốc => set đơn giá + tính lại
        $tr.find(".dt-thuoc").on("change", function () {
            const thuocId = parseInt($(this).val());
            const found = thuocListForDonThuoc.find(x => x.thuocId === thuocId);
            const gia = found ? found.giaBanLe : 0;
            $tr.find(".dt-dongia").val(gia);
            recalcRow();
            recalcTotals();
        });

        $tr.find(".dt-lieu, .dt-sothang").on("input", function () {
            recalcRow();
            recalcTotals();
        });

        $tr.find(".btn-row-remove").on("click", function () {
            $tr.remove();
            recalcTotals();
        });
    }
    $("#btn-add-donthuoc-row").on("click", function () {
        // Chưa load danh sách thuốc -> gửi request rồi thôi, chờ callback
        if (!thuocListLoadedForDonThuoc) {
            donThuocPendingAddRow = true;

            if (window.chrome && window.chrome.webview) {
                window.chrome.webview.postMessage({
                    action: "getThuocList",
                    data: {}
                });
            } else {
                alert("Không thể tải danh sách thuốc (WebView chưa sẵn sàng).");
            }

            return; // ❗ KHÔNG gọi addDonThuocRow ngay
        }

        // Đã load rồi thì thêm dòng bình thường
        addDonThuocRow();
    });
    // ===============================
    //  SAVE ĐƠN THUỐC (TẠO + SỬA)
    // ===============================
    $("#btn-donthuoc-save").click(function () {

        const header = collectDonThuocForm();
        const details = collectDonThuocDetails();

        const isEdit = header.DonThuocId > 0;

        window.chrome.webview.postMessage({
            action: isEdit ? "updateDonThuoc" : "createDonThuoc",
            data: {
                Header: {
                    ...header,
                    CreatedBy: App.currentUser?.userId || 1,   // 🔹 thêm dòng này
                    UserId: App.currentUser?.userId || 1       // (nếu backend dùng cột UserId)
                },
                Details: details
            }
        });
    });


    // ===============================
    //  NÚT SỬA ĐƠN THUỐC
    // ===============================


    // ===============================
    //  CALLBACK KHI TẠO ĐƠN
    // ===============================
    window.App.onDonThuocCreated = function (res) {
        alert("Tạo đơn thành công! Mã đơn: " + res.DonThuocId);
        requestDonThuocList();
    };

    window.App.onDonThuocUpdated = function () {
        alert("Cập nhật đơn thành công!");
        requestDonThuocList();
    };


    // ===============================
    //  LOAD DỮ LIỆU ĐỂ SỬA ĐƠN
    // ===============================
    // ===============================
    //  LOAD DỮ LIỆU ĐỂ SỬA ĐƠN
    // ===============================
    window.App.onDonThuocLoadForEdit = function (detail) {

        // ----- HEADER -----
        $("#dt-id").val(detail.donThuocId);

        $("#dt-da-thanh-toan").val(detail.daThanhToan);    // giữ lại số tiền đã thanh toán
        $("#dt-con-no").data("v", detail.conNo);         // nếu bạn đang hiển thị, set luôn data("v")
        $("#dt-phaitra").data("v", detail.tienKhachPhaiTra);
        $("#dt-tongtienhang").data("v", detail.tongTienHang);
        $("#dt-trangthai-don").val(detail.trangThaiDon ?? 0);
        $("#dt-trangthai-tt").val(detail.trangThaiThanhToan ?? 0);
        // Khách hàng: nếu option chưa tồn tại thì thêm vào
        const $khach = $("#dt-khachhang");
        if ($khach.length) {
            const khId = detail.khachHangId;
            const khText =
                (detail.maKhachHang ? detail.maKhachHang + " - " : "") +
                (detail.tenKhachHang || "");

            if (khId && $khach.find("option[value='" + khId + "']").length === 0) {
                $khach.append(
                    `<option value="${khId}">${khText}</option>`
                );
            }
            $khach.val(String(khId));
        }

        $("#dt-ngaylap").val(detail.ngayLap.split("T")[0]).prop("disabled", true);

        $("#dt-bacsi").val(detail.bacSiKeDon || "");
        $("#dt-chandoan").val(detail.chanDoan || "");
        const $bs = $("#dt-bacsi");
        if ($bs.length) {
            const name = detail.bacSiKeDon || "";

            // thêm vào dropdown nếu chưa có
            if (name && $bs.find("option[value='" + name + "']").length === 0) {
                $bs.append(new Option(name, name, true, true));
            }
            $bs.val(name);

            const roleName = (App.currentUser?.roleName || "").toLowerCase();
            const isAdmin = roleName === "admin";

            // Admin vẫn được chỉnh; non-admin thì khóa theo rule
            $bs.prop("disabled", !isAdmin);
        }

        // Giảm giá khi sửa: cho nhập luôn (trừ khi bạn cố ý khóa)
        $("#dt-giamgia")
            .val("0")
            .prop("disabled", false)
            .prop("readonly", false);
        // ----- CHI TIẾT DÒNG -----
        const tbody = $("#table-donthuoc-details tbody");
        tbody.html("");

        (detail.chiTiet || []).forEach(x => {
            const thanhTien = x.thanhTien ??
                ((x.donGiaBan || 0) * (x.soThang || 0));

            tbody.append(`
            <tr>
                <td>
                    <select class="input dt-thuoc">
                        <option value="${x.thuocId}">${x.tenThuoc}</option>
                    </select>
                </td>
                <td><input class="input dt-lieu" value="${x.lieuLuongGram || ""}" /></td>
                <td><input class="input dt-sothang" value="${x.soThang || ""}" /></td>
                <td><input class="input dt-dongia" value="${x.donGiaBan || ""}" readonly /></td>
                <td class="dt-thanhtien" data-v="${thanhTien}">
                    ${thanhTien.toLocaleString("vi-VN")}
                </td>
                <td>
                    <button class="btn btn-sm btn-danger btn-row-remove">X</button>
                </td>
            </tr>
        `);
        });

        // ----- TỔNG TIỀN / GIẢM GIÁ / CÔN NỢ -----
        let tongTienHang = detail.tongTienHang;
        if (tongTienHang == null) {
            tongTienHang = 0;
            (detail.chiTiet || []).forEach(x => {
                tongTienHang += x.thanhTien || 0;
            });
        }

        const giamGia = detail.giamGia || 0;
        const phaiTra = (detail.tienKhachPhaiTra != null)
            ? detail.tienKhachPhaiTra
            : (tongTienHang - giamGia);

        const daThanhToan = detail.daThanhToan;
        const conNo = (detail.conNo != null)
            ? detail.conNo
            : (phaiTra - daThanhToan);

        function setMoney(selector, value) {
            const $el = $(selector);
            if (!$el.length) return;
            $el.data("v", value);
            $el.text(value.toLocaleString("vi-VN") + " đ");
        }

        setMoney("#dt-tongtienhang", tongTienHang);
        $("#dt-giamgia").val(giamGia);
        setMoney("#dt-phaitra", phaiTra);
        setMoney("#dt-con-no", conNo);
        setMoney("#dt-da-thanh-toan", daThanhToan);
        // ----- MỞ MODAL SỬA -----
        $("#modal-donthuoc-title").text("Sửa đơn thuốc");
        $("#modal-add-donthuoc").addClass("open");

        applyDonThuocRolePermissions(true);
    };


    // =====================================================
    //  PHIẾU NHẬP KHO: XEM / IN
    // =====================================================
    const modalNhapKhoView = document.getElementById("modal-view-nhapkho");
    document.querySelectorAll(".btn-view-nhapkho, .btn-print-nhapkho").forEach(btn => {
        btn.addEventListener("click", () => {
            if (modalNhapKhoView) modalNhapKhoView.classList.add("open");
        });
    });


    // =====================================================
    //  PHIẾU THU / PHIẾU CHI (DEMO)
    // =====================================================
    const modalPhieuThu = document.getElementById("modal-phieuthu");
    document.querySelectorAll(".btn-thu-tien").forEach(btn => {
        btn.addEventListener("click", () => {
            if (modalPhieuThu) modalPhieuThu.classList.add("open");
        });
    });

    const modalPhieuChi = document.getElementById("modal-phieuchi");
    document.querySelectorAll(".btn-thanh-toan-ncc").forEach(btn => {
        btn.addEventListener("click", () => {
            if (modalPhieuChi) modalPhieuChi.classList.add("open");
        });
    });

    const btnPhieuThuSave = document.getElementById("btn-phieuthu-save");
    if (btnPhieuThuSave) {
        btnPhieuThuSave.addEventListener("click", () => {
           
        });
    }

    const btnPhieuChiSave = document.getElementById("btn-phieuchi-save");
    if (btnPhieuChiSave) {
        btnPhieuChiSave.addEventListener("click", () => {
        });
    }

    // =====================================================
    //  LOAD DANH SÁCH THUỐC TỪ C#
    // =====================================================
    function requestThuocList() {
        const payload = { action: "getThuocList", data: {} };

        if (window.chrome && window.chrome.webview) {
            window.chrome.webview.postMessage(payload);
        } else {
            const demo = [
                {
                    thuocId: 1,
                    maThuoc: "T001",
                    tenThuoc: "Hoàng kỳ",
                    donViTinh: "gram",
                    giaBanLe: 50000,
                    tonToiThieu: 1000,
                    ghiChu: "Thuốc bổ khí (demo)"
                },
                {
                    thuocId: 2,
                    maThuoc: "T002",
                    tenThuoc: "Cam thảo",
                    donViTinh: "gram",
                    giaBanLe: 45000,
                    tonToiThieu: 800,
                    ghiChu: "Điều hoà các vị thuốc (demo)"
                }
            ];
            App.onThuocList(demo);
        }
    }



    App.onThuocSaved = function (result) {
        // result = { success: bool, message: string }
        if (result && result.message) {
            alert(result.message);
        }

        // Nếu lưu thành công thì đóng modal + clear id
        if (result && result.success) {
            const thuocModal = document.getElementById("modal-add-thuoc");
            const thuocIdInput = document.getElementById("thuoc-id");

            if (thuocIdInput) thuocIdInput.value = "";
            if (thuocModal) {
                thuocModal.classList.remove("open");
            }

            // Danh sách đã được C# reload rồi (HandleGetThuocListAsync),
            // nên không cần gọi lại requestThuocList() nữa.
        }
    };

    // ===============================
    //  KHÁCH HÀNG – LOAD LIST
    // ===============================
    // =====================================================
    //  LOAD DANH SÁCH KHÁCH HÀNG TỪ C#
    // =====================================================
    function requestKhachList() {
        const payload = { action: "getKhachList", data: {} };

        if (window.chrome && window.chrome.webview) {
            window.chrome.webview.postMessage(payload);
        } else {
            const demo = [
                {
                    khachHangId: 1,
                    maKhachHang: "KH001",
                    hoTen: "Nguyễn Văn A",
                    namSinh: 1985,
                    dienThoai: "0123456789",
                    diaChi: "TP. HCM",
                    ghiChu: "Huyết áp cao"
                }
            ];
            App.onKhachList(demo);
        }
    }
    let khachListLoadedForDonThuoc = false;

    let khachListAll = [];
    window.App = window.App || {};

    App.onKhachList = function (items) {
        // Cache toàn bộ danh sách khách hàng để phục vụ search/filter
        khachListAll = items || [];

        // 1. Render bảng Khách hàng theo filter hiện tại (ô search "khach-search")
        // Hàm này sẽ tự:
        // - lọc theo keyword
        // - vẽ lại tbody
        // - gọi bindKhachRowEvents() bên trong
        renderKhachTable();

        // 2. Đổ dữ liệu vào combo Đơn thuốc
        const $sel = $("#dt-khachhang");
        if ($sel.length) {
            $sel.html('<option value="">-- Chọn khách hàng --</option>');
            (khachListAll || []).forEach(k => {
                const text =
                    (k.maKhachHang ? k.maKhachHang + " - " : "") +
                    (k.hoTen || "");
                $sel.append(`<option value="${k.khachHangId}">${text}</option>`);
            });
            khachListLoadedForDonThuoc = true;
        }
    };




    function getKhachSearchText() {
        const input = document.getElementById("khach-search");
        return (input?.value || "").trim().toLowerCase();
    }

    function filterKhachList(list) {
        const keyword = getKhachSearchText();
        if (!keyword) return list || [];

        return (list || []).filter(k => {
            const ten = (k.hoTen || "").toLowerCase();
            const ma = (k.maKhachHang || "").toLowerCase();
            const sdt = (k.dienThoai || "").toLowerCase();
            return ten.includes(keyword) || ma.includes(keyword) || sdt.includes(keyword);
        });
    }

    function renderKhachTable() {
        const tbody = document.querySelector("#page-khachhang table tbody");
        if (!tbody) return;

        const items = filterKhachList(khachListAll);

        tbody.innerHTML = "";

        if (!items || !items.length) {
            const tr = document.createElement("tr");
            tr.innerHTML = "<td colspan='7'>Chưa có dữ liệu khách hàng.</td>";
            tbody.appendChild(tr);
            bindKhachRowEvents();
            return;
        }

        items.forEach((k, index) => {
            const tr = document.createElement("tr");
            tr.dataset.khachId = k.khachHangId;
            tr.dataset.maKhach = k.maKhachHang || "";
            tr.dataset.hoTen = k.hoTen || "";
            tr.dataset.namSinh = k.namSinh || "";
            tr.dataset.gioiTinh = k.gioiTinh ?? "";
            tr.dataset.dienThoai = k.dienThoai || "";
            tr.dataset.email = k.email || "";
            tr.dataset.diaChi = k.diaChi || "";
            tr.dataset.ghiChu = k.ghiChu || "";
            tr.innerHTML = `
            <td>${index + 1}</td>
            <td>${k.maKhachHang}</td>
            <td>${k.hoTen}</td>
            <td>${k.namSinh || ""}</td>
            <td>${k.dienThoai || ""}</td>
            <td>${k.diaChi || ""}</td>
            <td>
                <button class="btn btn-sm btn-edit-khach">Sửa</button>
                <button class="btn btn-sm btn-danger btn-delete-khach">Xóa</button>
            </td>`;
            tbody.appendChild(tr);
        });

        bindKhachRowEvents();
    }

    function bindKhachRowEvents() {
        const khachModal = document.getElementById("modal-add-khach");
        const khachModalTitle = document.getElementById("modal-khach-title");

        const khachIdInput = document.getElementById("khach-id");
        const khachMaInput = document.getElementById("khach-ma");
        const khachTenInput = document.getElementById("khach-ten");
        const khachNamSinhInput = document.getElementById("khach-namsinh");
        const khachGioiTinhInput = document.getElementById("khach-gioitinh");
        const khachDienThoaiInput = document.getElementById("khach-dienthoai");
        const khachEmailInput = document.getElementById("khach-email");
        const khachDiaChiInput = document.getElementById("khach-diachi");
        const khachGhiChuInput = document.getElementById("khach-ghichu");

        // SỬA KHÁCH
        document.querySelectorAll("#page-khachhang .btn-edit-khach").forEach(btn => {
            btn.onclick = () => {
                const row = btn.closest("tr");
                if (!row || !khachModal || !khachModalTitle) return;

                if (khachIdInput) khachIdInput.value = row.dataset.khachId || "0";
                if (khachMaInput) khachMaInput.value = row.dataset.maKhach || "";
                if (khachTenInput) khachTenInput.value = row.dataset.hoTen || "";
                if (khachNamSinhInput) khachNamSinhInput.value = row.dataset.namSinh || "";
                if (khachGioiTinhInput) khachGioiTinhInput.value = row.dataset.gioiTinh || "";
                if (khachDienThoaiInput) khachDienThoaiInput.value = row.dataset.dienThoai || "";
                if (khachEmailInput) khachEmailInput.value = row.dataset.email || "";
                if (khachDiaChiInput) khachDiaChiInput.value = row.dataset.diaChi || "";
                if (khachGhiChuInput) khachGhiChuInput.value = row.dataset.ghiChu || "";

                khachModalTitle.textContent = "Sửa khách hàng";
                khachModal.classList.add("open");
            };

        });

        // XÓA KHÁCH – gọi xuống C#
        document.querySelectorAll("#page-khachhang .btn-delete-khach").forEach(btn => {
            btn.onclick = () => {
                const row = btn.closest("tr");
                if (!row) return;

                const id = parseInt(row.dataset.khachId || "0", 10);
                const ma = row.children[1].textContent.trim();  // cột Mã KH
                const ten = row.children[2].textContent.trim(); // cột Họ tên

                if (!confirm(`Bạn có chắc chắn muốn xóa khách hàng ${ma} - ${ten}?`)) {
                    return;
                }

                const payload = {
                    action: "deleteKhach",
                    data: { khachHangId: id }
                };

                if (window.chrome && window.chrome.webview) {
                    window.chrome.webview.postMessage(payload);
                } else {
                    row.remove();
                }
            };
        });

    }


    // ❌ bỏ dòng bindKhachRowEvents(); ở cuối file (nếu còn)
    // bindKhachRowEvents();


    // gọi 1 lần cho dữ liệu tĩnh ban đầu (nếu có)
    bindKhachRowEvents();

    function bindThuocRowEvents() {
        const thuocModal = document.getElementById("modal-add-thuoc");
        const thuocModalTitle = document.getElementById("modal-thuoc-title");

        const thuocIdInput = document.getElementById("thuoc-id");
        const thuocMaInput = document.getElementById("thuoc-ma");
        const thuocTenInput = document.getElementById("thuoc-ten");
        const thuocTenKhac = document.getElementById("thuoc-tenkhac");
        const thuocDonViSel = document.getElementById("thuoc-donvi");
        const thuocGiaInput = document.getElementById("thuoc-giaban");
        const thuocTonInput = document.getElementById("thuoc-ton");          // = Tồn tối thiểu
        const thuocCongDung = document.getElementById("thuoc-congdung");
        const thuocCCD = document.getElementById("thuoc-chongchidinh");
        const thuocGhiChu = document.getElementById("thuoc-ghichu");

        // ====== SỬA THUỐC ======
        document.querySelectorAll("#page-thuoc .btn-edit-thuoc").forEach(btn => {
            btn.onclick = function () {
                if (!thuocModal || !thuocModalTitle) return;

                const row = btn.closest("tr");
                if (!row) return;

                const cells = row.querySelectorAll("td");

                const ma = cells[0].textContent.trim();
                const ten = cells[1].textContent.trim();
                const donvi = cells[2].textContent.trim();
                const gia = cells[3].textContent.replace(/[^\d]/g, "");

                // 🔥 Dùng data-* thay vì lấy theo index cột
                const tonToiThieu = row.dataset.tonToiThieu || "0";
                const tenKhac = row.dataset.tenKhac || "";
                const congDung = row.dataset.congDung || "";
                const chongChiDinh = row.dataset.chongChiDinh || "";
                const ghiChu = row.dataset.ghiChu || "";

                if (thuocIdInput) thuocIdInput.value = row.dataset.thuocId || "";
                if (thuocMaInput) thuocMaInput.value = ma;
                if (thuocTenInput) thuocTenInput.value = ten;
                if (thuocDonViSel) thuocDonViSel.value = donvi;
                if (thuocGiaInput) thuocGiaInput.value = gia;

                // Tồn tối thiểu
                if (thuocTonInput) thuocTonInput.value = tonToiThieu;

                if (thuocTenKhac) thuocTenKhac.value = tenKhac;
                if (thuocCongDung) thuocCongDung.value = congDung;
                if (thuocCCD) thuocCCD.value = chongChiDinh;

                // Ghi chú đúng dữ liệu, không còn bị 0 nữa
                if (thuocGhiChu) thuocGhiChu.value = ghiChu;

                thuocModalTitle.textContent = "Sửa thuốc Đông y";
                thuocModal.classList.add("open");
            };
        });

        // ====== XÓA THUỐC ======
        document.querySelectorAll(".btn-delete-thuoc").forEach(btn => {
            btn.addEventListener("click", () => {
                const row = btn.closest("tr");
                if (!row) return;

                pendingDeleteRow = row;
                pendingDeleteType = "thuoc";
                pendingDeleteId = parseInt(row.dataset.thuocId || "0", 10);

                const code = row.children[0]?.textContent.trim() || "";
                const name = row.children[1]?.textContent.trim() || "";

                if (confirmMessage) {
                    confirmMessage.textContent = `Bạn có chắc chắn muốn xóa thuốc "${code} - ${name}"?`;
                }
                if (confirmModal) confirmModal.classList.add("open");
            });
        });
    }

    const btnThuocSave = document.getElementById("btn-thuoc-save");
    if (btnThuocSave) {
        btnThuocSave.addEventListener("click", () => {
            const id = parseInt(document.getElementById("thuoc-id").value || "0", 10);
            const ma = (document.getElementById("thuoc-ma").value || "").trim();
            const ten = (document.getElementById("thuoc-ten").value || "").trim();

            if (!ten) {
                alert("Vui lòng nhập Tên thuốc.");
                return;
            }

            const tenKhac = (document.getElementById("thuoc-tenkhac").value || "").trim();
            const donViTinh = (document.getElementById("thuoc-donvi").value || "").trim();
            const giaBanLe = parseFloat(document.getElementById("thuoc-giaban").value || "0");
            const tonToiThieu = parseFloat(document.getElementById("thuoc-ton").value || "0");
            const congDung = (document.getElementById("thuoc-congdung").value || "").trim();
            const chongChiDinh = (document.getElementById("thuoc-chongchidinh").value || "").trim();
            const ghiChu = (document.getElementById("thuoc-ghichu").value || "").trim();

            const payload = {
                action: id > 0 ? "updateThuoc" : "createThuoc",
                data: {
                    thuocId: id,
                    tenThuoc: ten,
                    tenKhac,
                    donViTinh,
                    giaBanLe,
                    tonToiThieu,
                    congDung,
                    chongChiDinh,
                    ghiChu
                }
            };

            if (window.chrome && window.chrome.webview) {
                window.chrome.webview.postMessage(payload);
            } else {
                console.log("save thuoc:", payload);
            }
        });
    }
    const btnKhachSave = document.getElementById("btn-khach-save");
    if (btnKhachSave) {
        btnKhachSave.addEventListener("click", () => {
            const id = parseInt(document.getElementById("khach-id").value || "0", 10);
            const ma = (document.getElementById("khach-ma").value || "").trim();
            const ten = (document.getElementById("khach-ten").value || "").trim();

            if (!ten) {
                alert("Vui lòng nhập tên khách hàng.");
                return;
            }

            const namStr = (document.getElementById("khach-namsinh").value || "").trim();
            const namSinh = namStr ? parseInt(namStr, 10) : null;

            const gioiStr = (document.getElementById("khach-gioitinh").value || "").trim();
            const gioiTinh = gioiStr ? parseInt(gioiStr, 10) : null;

            const dienThoai = (document.getElementById("khach-dienthoai").value || "").trim();
            const email = (document.getElementById("khach-email").value || "").trim();
            const diaChi = (document.getElementById("khach-diachi").value || "").trim();
            const ghiChu = (document.getElementById("khach-ghichu").value || "").trim();

            const payload = {
                action: id > 0 ? "updateKhach" : "createKhach",
                data: {
                    khachHangId: id,
                    hoTen: ten,
                    namSinh: namSinh,
                    gioiTinh: gioiTinh,
                    dienThoai,
                    email,
                    diaChi,
                    ghiChu
                }
            };

            if (window.chrome && window.chrome.webview) {
                window.chrome.webview.postMessage(payload);
            } else {
                console.log("DEMO save khach:", payload);
            }

            const khachModal = document.getElementById("modal-add-khach");
            if (khachModal) khachModal.classList.remove("open");
        });
    }
    /// nhap kho
    let nccList = []; 
    App.onNccList = function (items) {
        nccList = items || [];

        // fill datalist cho phiếu nhập
        const dl = document.getElementById("pn-ncc-suggestions");
        if (!dl) return;

        dl.innerHTML = "";
        nccList.forEach(n => {
            const opt = document.createElement("option");
            opt.value = `${n.maNcc || ""} - ${n.tenNcc || ""}`.trim();
            dl.appendChild(opt);
        });
    };
    function recalcNhapKhoTotals() {
        const tbody = document.querySelector("#table-nhapkho-details tbody");
        if (!tbody) return;
        let tong = 0;

        tbody.querySelectorAll(".nk-thanhtien").forEach(td => {
            tong += Number(td.dataset.v) || 0;
        });

        const label = document.getElementById("pn-tongtien-text");
        if (label) {
            label.textContent = tong.toLocaleString("vi-VN") + " đ";
        }
    }

    function addNhapKhoRow() {
        const tbody = document.querySelector("#table-nhapkho-details tbody");
        if (!tbody) return;

    // 1) Chưa hề load danh sách thuốc từ C#
    if (!thuocListLoadedForDonThuoc) {
        if (window.chrome && window.chrome.webview) {
            window.chrome.webview.postMessage({
                action: "getThuocList",
                data: {}
            });
        }
        // đánh dấu: khi load xong sẽ tự gọi lại addNhapKhoRow
        nhapKhoPendingAddRow = true;
        return;
    }

    // 2) ĐÃ load mà list vẫn rỗng => trong DB thật sự chưa có thuốc nào
    if (!thuocListForDonThuoc.length) {
        alert("Chưa có danh sách thuốc, hãy vào Danh mục thuốc để thêm thuốc trước.");
        return;
    }

        const optionsHtml = thuocListForDonThuoc.map(t => `
        <option value="${t.thuocId}" data-donvi="${t.donViTinh || ""}">
            ${t.tenThuoc}
        </option>
    `).join("");

        const tr = document.createElement("tr");
        tr.innerHTML = `
        <td>
            <select class="input nk-thuoc">
                <option value="">-- Chọn thuốc --</option>
                ${optionsHtml}
            </select>
        </td>
        <td><input class="input nk-soluong" value="0" /></td>
        <td><input class="input nk-dongia" value="0" /></td>
        <td class="nk-thanhtien" data-v="0">0</td>
        <td>
            <button class="btn btn-sm btn-danger nk-row-remove" type="button">X</button>
        </td>
    `;

        tbody.appendChild(tr);

        // hàm tính lại thành tiền cho 1 dòng
        function recalcRow() {
            const sl = parseFloat(tr.querySelector(".nk-soluong").value || "0") || 0;
            const dg = parseFloat(tr.querySelector(".nk-dongia").value || "0") || 0;
            const tt = sl * dg;

            const td = tr.querySelector(".nk-thanhtien");
            td.dataset.v = tt;
            td.textContent = tt.toLocaleString("vi-VN");

            recalcNhapKhoTotals();
        }

        tr.querySelector(".nk-soluong").addEventListener("input", recalcRow);
        tr.querySelector(".nk-dongia").addEventListener("input", recalcRow);
        tr.querySelector(".nk-row-remove").addEventListener("click", () => {
            tr.remove();
            recalcNhapKhoTotals();
        });
    }


    App.onNccSaved = function (res) {
        if (!res || !res.success) {
            alert("Lưu nhà cung cấp thất bại: " + (res?.message || "Không rõ lỗi"));
            return;
        }

        // C# đã reload list => App.onNccList đã chạy => nccList mới
        if (quickCreateFromNhapKho) {
            quickCreateFromNhapKho = false;

            const modalQuick = document.getElementById("modal-quick-ncc");
            if (modalQuick) modalQuick.classList.remove("open");

            const newId = res.nhaCungCapId;
            const ncc = nccList.find(x => x.nhaCungCapId === newId);

            const nameInput = document.getElementById("pn-ncc-name");
            const idInput = document.getElementById("pn-ncc-id");

            if (ncc && nameInput && idInput) {
                nameInput.value = `${ncc.maNcc || ""} - ${ncc.tenNcc || ""}`.trim();
                idInput.value = String(newId);
            }

            document.getElementById("modal-add-nhapkho")?.classList.add("open");
        } else {
            // Trường hợp lưu/sửa NCC từ màn danh mục NCC
            alert("Đã lưu nhà cung cấp.");
            document.getElementById("modal-add-ncc")?.classList.remove("open");
        }
    };


    App.onNccDeleted = function (res) {
        if (!res || !res.success) {
            alert("Xóa nhà cung cấp thất bại: " + (res?.message || "Không rõ lỗi"));
        }
    };
    function resetNccForm() {
        const modal = document.getElementById("modal-add-ncc");
        if (!modal) return;
        modal.dataset.returnToNhapKho = "";

        const ids = ["ncc-id", "ncc-ma", "ncc-ten", "ncc-lienhe",
            "ncc-dienthoai", "ncc-email", "ncc-diachi", "ncc-ghichu"];

        ids.forEach(id => {
            const el = document.getElementById(id);
            if (el) el.value = "";
        });

        const title = document.getElementById("modal-ncc-title");
        if (title) title.textContent = "Thêm nhà cung cấp";
    }

    function collectNccForm() {
        const id = parseInt(document.getElementById("ncc-id")?.value || "0", 10);
        const ma = document.getElementById("ncc-ma")?.value.trim() || "";
        const ten = document.getElementById("ncc-ten")?.value.trim() || "";
        const nguoi = document.getElementById("ncc-lienhe")?.value.trim() || "";
        const dt = document.getElementById("ncc-dienthoai")?.value.trim() || "";
        const email = document.getElementById("ncc-email")?.value.trim() || "";
        const diachi = document.getElementById("ncc-diachi")?.value.trim() || "";
        const ghichu = document.getElementById("ncc-ghichu")?.value.trim() || "";

        if (!ma || !ten) {
            alert("Mã NCC và Tên NCC là bắt buộc.");
            return null;
        }

        return {
            nhaCungCapId: id,
            maNcc: ma,
            tenNcc: ten,
            nguoiLienHe: nguoi,
            dienThoai: dt,
            email: email,
            diaChi: diachi,
            ghiChu: ghichu
        };
    }

    function bindNccRowEvents() {
        const modal = document.getElementById("modal-add-ncc");
        const btnAdd = document.querySelector('button[data-modal-target="modal-add-ncc"]');

        if (btnAdd && modal) {
            btnAdd.onclick = function () {
                resetNccForm();
                modal.classList.add("open");
            };
        }

        const tbody = document.querySelector("#page-ncc table tbody");
        if (!tbody) return;

        // Sửa
        tbody.querySelectorAll(".btn-edit-ncc").forEach(btn => {
            btn.onclick = () => {
                const row = btn.closest("tr");
                if (!row || !modal) return;

                resetNccForm();

                const id = row.dataset.nhaCungCapId || "0";
                const cells = row.querySelectorAll("td");

                document.getElementById("ncc-id").value = id;
                document.getElementById("ncc-ma").value = cells[0].textContent.trim();
                document.getElementById("ncc-ten").value = cells[1].textContent.trim();
                document.getElementById("ncc-lienhe").value = cells[2].textContent.trim();
                document.getElementById("ncc-dienthoai").value = cells[3].textContent.trim();
                document.getElementById("ncc-email").value = cells[4].textContent.trim();
                document.getElementById("ncc-diachi").value = cells[5].textContent.trim();
                // ghi chú không hiển thị trong list, nếu cần có thể lấy từ data-*

                const title = document.getElementById("modal-ncc-title");
                if (title) title.textContent = "Sửa nhà cung cấp";

                modal.classList.add("open");
            };
        });

        // Xóa
        tbody.querySelectorAll(".btn-delete-ncc").forEach(btn => {
            btn.onclick = () => {
                const row = btn.closest("tr");
                if (!row) return;
                const id = parseInt(row.dataset.nhaCungCapId || "0", 10);
                const name = row.children[1]?.textContent.trim() || "";

                if (!id) return;
                if (!confirm(`Bạn có chắc chắn muốn xóa NCC "${name}"?`)) return;

                if (window.chrome && window.chrome.webview) {
                    window.chrome.webview.postMessage({
                        action: "deleteNcc",
                        data: { nhaCungCapId: id }
                    });
                }
            };
        });

        // Lưu từ modal
        const btnSave = document.getElementById("btn-ncc-save");
        if (btnSave) {
            btnSave.onclick = () => {
                const dto = collectNccForm();
                if (!dto) return;

                const isEdit = dto.nhaCungCapId > 0;
                if (window.chrome && window.chrome.webview) {
                    window.chrome.webview.postMessage({
                        action: isEdit ? "updateNcc" : "createNcc",
                        data: dto
                    });
                }
            };
        }
    }
    function syncNccFromInput() {
        const nameInput = document.getElementById("pn-ncc-name");
        const idInput = document.getElementById("pn-ncc-id");
        if (!nameInput || !idInput) return;

        const text = nameInput.value.trim();
        idInput.value = "0";
        if (!text) return;

        const lower = text.toLowerCase();
        let exact = null;

        for (const n of nccList) {
            const full = `${n.maNcc || ""} - ${n.tenNcc || ""}`.trim();
            const fullLower = full.toLowerCase();
            const tenLower = (n.tenNcc || "").toLowerCase();

            if (fullLower === lower || tenLower === lower) {
                exact = n;
                break;
            }
        }

        if (exact) {
            idInput.value = String(exact.nhaCungCapId);
        }
        console.log("syncNccFromInput:", { text, id: idInput.value, match: exact });
    }
    initDonThuocFilters();
    initPhieuNhapFilters();


    // gắn event

    function openQuickCreateNcc(prefillName) {
        const modalNcc = document.getElementById("modal-add-ncc");
        if (!modalNcc) return;

        // đánh dấu là tạo nhanh từ phiếu nhập
        modalNcc.dataset.returnToNhapKho = "1";

        // reset form NCC (tuỳ bạn đã có hàm, ví dụ resetNccForm)
        if (typeof resetNccForm === "function") resetNccForm();

        const tenInput = document.getElementById("ncc-ten");
        if (tenInput) tenInput.value = prefillName || "";

        const title = document.getElementById("modal-ncc-title");
        if (title) title.textContent = "Thêm nhà cung cấp";

        document.getElementById("modal-add-nhapkho")?.classList.remove("open");
        modalNcc.classList.add("open");
    }

    function requestPhieuNhapList() {
        if (window.chrome && window.chrome.webview) {
            window.chrome.webview.postMessage({
                action: "getPhieuNhapList",
                data: {}
            });
        } else {
            console.log("DEMO getPhieuNhapList");
        }
    }
    App.onPhieuNhapList = function (items) {
        phieuNhapListAll = items || [];
        renderPhieuNhapTable();
    };
    function initDonThuocFilters() {
        const container = document.querySelector("#page-donthuoc .filters-row");
        if (!container) return;

        const inputs = container.querySelectorAll("input");
        inputs.forEach(input => {
            input.addEventListener("change", () => renderDonThuocTable());
            input.addEventListener("keyup", (e) => {
                // muốn realtime thì bỏ if này đi
                if (e.key === "Enter") {
                    renderDonThuocTable();
                }
            });
        });
    }

    function initPhieuNhapFilters() {
        const container = document.querySelector("#page-nhapkho .filters-row");
        if (!container) return;

        const inputs = container.querySelectorAll("input");
        inputs.forEach(input => {
            input.addEventListener("change", () => renderPhieuNhapTable());
            input.addEventListener("keyup", (e) => {
                if (e.key === "Enter") {
                    renderPhieuNhapTable();
                }
            });
        });
    }

    function bindNhapKhoRowEvents() {
        const page = document.getElementById("page-nhapkho");
        if (!page) return;

        const tbody = page.querySelector("tbody");
        if (!tbody) return;

        // ====== NÚT XEM CHI TIẾT PHIẾU NHẬP ======
        tbody.querySelectorAll(".btn-view-nhapkho").forEach(btn => {
            btn.onclick = () => {
                const tr = btn.closest("tr");
                if (!tr) return;
                const id = parseInt(tr.dataset.phieuNhapId || "0", 10);
                if (!id) return;

                if (window.chrome && window.chrome.webview) {
                    window.chrome.webview.postMessage({
                        action: "getPhieuNhapDetail",
                        data: { phieuNhapId: id }
                    });
                }
            };
        });

        // ====== NÚT THANH TOÁN NCC (MỞ MODAL PHIẾU CHI) ======
        tbody.querySelectorAll(".btn-thanh-toan-ncc").forEach(btn => {
            btn.onclick = () => {
                const tr = btn.closest("tr");
                if (!tr) return;

                const modal = document.getElementById("modal-phieuchi");
                if (!modal) return;

                const tenNcc = tr.dataset.tenNcc || "";
                const soPhieu = tr.children[0]?.textContent.trim() || "";
                const tienPhaiTra = parseFloat(tr.dataset.tienPhaiTra || "0");
                const daThanhToan = parseFloat(tr.dataset.daThanhToan || "0");
                const conNo = parseFloat(tr.dataset.conNo || "0");

                // rất quan trọng: gán id phiếu nhập vào modal để lúc lưu dùng
                modal.dataset.phieuNhapId = tr.dataset.phieuNhapId || "0";

                // ⚠️ Đoạn dưới này đang match theo value cứng trong HTML demo.
                // Dùng tạm, sau này nên đổi sang dùng id cho sạch.
                modal.querySelector('input[placeholder="Cơ sở Dược liệu An Nhiên"]')?.remove();

                const inpTenNcc = modal.querySelector("input[disabled][value='Cơ sở Dược liệu An Nhiên']");
                if (inpTenNcc) inpTenNcc.value = tenNcc;

                const inpSoPhieu = modal.querySelector("input[disabled][value='PN2025-0010']");
                if (inpSoPhieu) inpSoPhieu.value = soPhieu;

                const inpPhaiTra = modal.querySelector("input[disabled][value='195.000.000']");
                if (inpPhaiTra) inpPhaiTra.value = tienPhaiTra.toLocaleString("vi-VN");

                const inpDaTra = modal.querySelector("input[disabled][value='150.000.000']");
                if (inpDaTra) inpDaTra.value = daThanhToan.toLocaleString("vi-VN");

                const inpSoTien = document.getElementById("phieuchi-sotien");
                if (inpSoTien) {
                    inpSoTien.value = conNo.toLocaleString("vi-VN");
                }

                modal.classList.add("open");
            };
        });

        // ====== NÚT IN PHIẾU NHẬP ======
        tbody.querySelectorAll(".btn-print-nhapkho").forEach(btn => {
            btn.onclick = () => {
                const tr = btn.closest("tr");
                if (!tr) return;
                const id = parseInt(tr.dataset.phieuNhapId || "0", 10);
                if (!id) return;

                if (window.chrome && window.chrome.webview) {
                    window.chrome.webview.postMessage({
                        action: "printPhieuNhap",
                        data: { phieuNhapId: id }
                    });
                } else {
                    alert("In phiếu nhập ");
                }
            };
        });


        // ====== LƯU PHIẾU CHI (THANH TOÁN NCC) ======
        const btnPhieuChiSave = document.getElementById("btn-phieuchi-save");
        if (btnPhieuChiSave) {
            btnPhieuChiSave.onclick = () => {
                const modal = document.getElementById("modal-phieuchi");
                if (!modal) return;

                const id = parseInt(modal.dataset.phieuNhapId || "0", 10);
                if (!id) {
                    alert("Không xác định được phiếu nhập cần thanh toán.");
                    return;
                }

                const soTienStr = (document.getElementById("phieuchi-sotien")?.value || "0")
                    .replace(/[^\d]/g, "");
                const soTien = parseFloat(soTienStr) || 0;
                if (soTien <= 0) {
                    alert("Số tiền trả phải > 0");
                    return;
                }

                // tạm thời: 1 = tiền mặt
                const hinhThuc = 1;
                const ghiChu = modal.querySelector("textarea")?.value || "";

                if (window.chrome && window.chrome.webview) {
                    window.chrome.webview.postMessage({
                        action: "payPhieuNhap",
                        data: {
                            phieuNhapId: id,
                            soTien: soTien,
                            hinhThuc: hinhThuc,
                            ghiChu: ghiChu
                        }
                    });
                } else {
                    console.log("DEMO payPhieuNhap", { id, soTien, hinhThuc, ghiChu });
                }
            };
        }
    }

    App.onPhieuNhapSaved = function (res) {
        if (!res || !res.success) {
            alert("Lưu phiếu nhập thất bại: " + (res?.message || "Không rõ lỗi"));
            return;
        }
        alert("Đã lưu phiếu nhập thành công.");
        document.getElementById("modal-add-nhapkho")?.classList.remove("open");
    };

    App.onPhieuNhapPaid = function (res) {
        if (!res || !res.success) {
            alert("Thanh toán NCC thất bại: " + (res?.message || "Không rõ lỗi"));
            return;
        }
        alert("Đã lưu phiếu chi / cập nhật công nợ NCC.");
        document.getElementById("modal-phieuchi")?.classList.remove("open");
    };
    function resetNhapKhoForm() {
        document.getElementById("pn-sophieu").value = "";
        document.getElementById("pn-ngaynhap").valueAsDate = new Date();
        document.getElementById("pn-ncc-name").value = "";
        document.getElementById("pn-ncc-id").value = "0";
        document.getElementById("pn-ghichu").value = "";
        const tbody = document.querySelector("#table-nhapkho-details tbody");
        if (tbody) tbody.innerHTML = "";
        document.getElementById("pn-tongtien-text").textContent = "0 đ";
    }
    let nccInputBound = false; 
    let quickCreateFromNhapKho = false;
    function initNhapKhoModal() {
        const btnOpen = document.querySelector('button[data-modal-target="modal-add-nhapkho"]');
        const modal = document.getElementById("modal-add-nhapkho");
        if (!btnOpen || !modal) return;

        btnOpen.addEventListener("click", () => {
            resetNhapKhoForm();

            // yêu cầu load NCC + thuốc
            if (window.chrome && window.chrome.webview) {
                window.chrome.webview.postMessage({ action: "getNccList", data: {} });
                window.chrome.webview.postMessage({ action: "getThuocList", data: {} });
            }

            addNhapKhoRow(); // tạo sẵn 1 dòng
            modal.classList.add("open");
        });

        document.getElementById("btn-nhapkho-add-row")
            ?.addEventListener("click", addNhapKhoRow);

        // NCC input
        const nccInput = document.getElementById("pn-ncc-name");
        if (nccInput && !nccInputBound) {
            nccInputBound = true;

            nccInput.addEventListener("change", syncNccFromInput);

            nccInput.addEventListener("blur", function () {
                syncNccFromInput();

                const text = this.value.trim();
                const id = parseInt(document.getElementById("pn-ncc-id").value || "0", 10);

                // 🔥 Chỉ cần KHÔNG có trùng CHÍNH XÁC là mở quickcreate,
                // kể cả text chỉ là "cc" và trong list có nhiều NCC chứa "cc"
                if (text && !id) {
                    if (confirm(`Nhà cung cấp "${text}" chưa có. Bạn có muốn tạo nhanh?`)) {
                        openQuickCreateNcc(text);
                    }
                }
            });
        }



        // nút Lưu phiếu nhập gửi sang C#
        document.getElementById("btn-nhapkho-save")
            ?.addEventListener("click", () => {
                const dto = collectNhapKhoForm(); // bạn đang có / hoặc mình viết thêm
                if (!dto) return;

                if (window.chrome && window.chrome.webview) {
                    window.chrome.webview.postMessage({
                        action: "createPhieuNhap",
                        data: dto
                    });
                }
            });
    }


    function resetQuickNccForm() {
        ["quick-ncc-ma", "quick-ncc-ten", "quick-ncc-lienhe",
            "quick-ncc-dienthoai", "quick-ncc-email",
            "quick-ncc-diachi", "quick-ncc-ghichu"]
            .forEach(id => {
                const el = document.getElementById(id);
                if (el) el.value = "";
            });
    }

    function openQuickCreateNcc(prefillName) {
        const modalQuick = document.getElementById("modal-quick-ncc");
        if (!modalQuick) return;

        quickCreateFromNhapKho = true;
        resetQuickNccForm();

        const tenInput = document.getElementById("quick-ncc-ten");
        if (tenInput) tenInput.value = prefillName || "";

        document.getElementById("modal-add-nhapkho")?.classList.remove("open");
        modalQuick.classList.add("open");
    }
    function collectNhapKhoForm() {
        const modal = document.getElementById("modal-add-nhapkho");
        if (!modal) return null;

        // 🔹 Đồng bộ lại ID NCC dựa theo text hiện tại
        const nccInputEl = document.getElementById("pn-ncc-name");
        if (typeof syncNccFromInput === "function" && nccInputEl) {
            syncNccFromInput();
        }

        const soPhieu = modal.querySelector(".form-grid .field:nth-child(1) input").value.trim();
        const ngayNhap = modal.querySelector(".form-grid .field:nth-child(2) input").value;

        const nccName = document.getElementById("pn-ncc-name")?.value.trim() || "";
        const nccId = parseInt(document.getElementById("pn-ncc-id")?.value || "0", 10);

        const ghiChu = modal.querySelector(".form-grid .field:nth-child(4) input").value.trim();

        // 👀 Debug tạm nếu cần
        console.log("collectNhapKhoForm NCC:", { nccName, nccId });

        if (!ngayNhap) {
            alert("Vui lòng chọn ngày nhập.");
            return null;
        }
        if (!nccName || !nccId) {
            alert("Vui lòng chọn nhà cung cấp từ danh sách hoặc tạo mới.");
            return null;
        }

        const tbody = document.querySelector("#table-nhapkho-details tbody");
        const rows = tbody ? tbody.querySelectorAll("tr") : [];

        const details = [];
        rows.forEach(tr => {
            const selThuoc = tr.querySelector("select");
            const thuocId = parseInt(selThuoc?.value || "0", 10);
            if (!thuocId) return;

            const soLuong = parseFloat(tr.querySelector("td:nth-child(2) input").value || "0");
            const donGia = parseFloat(tr.querySelector("td:nth-child(3) input").value || "0");
            const thanhTien = soLuong * donGia;

            details.push({
                thuocId: thuocId,
                soLuong: soLuong,
                donGiaNhap: donGia,
                thanhTien: thanhTien,
                ghiChu: ""
            });
        });

        if (!details.length) {
            alert("Phiếu nhập phải có ít nhất 1 dòng thuốc.");
            return null;
        }

        const tongTien = details.reduce((sum, x) => sum + x.thanhTien, 0);

        return {
            header: {
                phieuNhapId: 0,
                nhaCungCapId: nccId,
                tenNhaCungCap: nccName,
                soPhieu: soPhieu,
                ngayNhap: ngayNhap,
                tongTienHang: tongTien,
                giamGia: 0,
                hanThanhToan: null,
                ghiChu: ghiChu
            },
            details: details
        };
    }

    document.getElementById("btn-quick-ncc-save")
        ?.addEventListener("click", () => {
            const ma = document.getElementById("quick-ncc-ma")?.value.trim() || "";
            const ten = document.getElementById("quick-ncc-ten")?.value.trim() || "";
            const nguoi = document.getElementById("quick-ncc-lienhe")?.value.trim() || "";
            const dt = document.getElementById("quick-ncc-dienthoai")?.value.trim() || "";
            const email = document.getElementById("quick-ncc-email")?.value.trim() || "";
            const diachi = document.getElementById("quick-ncc-diachi")?.value.trim() || "";
            const ghichu = document.getElementById("quick-ncc-ghichu")?.value.trim() || "";

            if (!ma || !ten) {
                alert("Mã NCC và Tên NCC là bắt buộc.");
                return;
            }

            const dto = {
                nhaCungCapId: 0,
                maNcc: ma,
                tenNcc: ten,
                nguoiLienHe: nguoi,
                dienThoai: dt,
                email: email,
                diaChi: diachi,
                ghiChu: ghichu
            };

            if (window.chrome && window.chrome.webview) {
                window.chrome.webview.postMessage({
                    action: "createNcc",
                    data: dto
                });
            }
        });

    App.onPhieuNhapDetail = function (detail) {
        const modal = document.getElementById("modal-view-nhapkho");
        if (!modal || !detail) return;

        modal.querySelector(".detail-title").textContent = "Tiệm thuốc Đông y";
        modal.querySelector(".detail-sub").textContent =
            `Phiếu nhập kho – Mã: ${detail.soPhieu}`;

        modal.querySelectorAll(".detail-value")[0].textContent = detail.tenNcc || "";
        modal.querySelectorAll(".detail-value")[1].textContent = detail.nguoiLienHe || "";
        modal.querySelectorAll(".detail-value")[2].textContent = formatDateVi(detail.ngayNhap);
        modal.querySelectorAll(".detail-value")[3].textContent = detail.ghiChu || "";

        const tbody = modal.querySelector(".detail-block table tbody");
        if (tbody) {
            tbody.innerHTML = "";
            (detail.chiTiet || []).forEach((x, idx) => {
                const tr = document.createElement("tr");
                tr.innerHTML = `
                <td>${idx + 1}</td>
                <td>${x.tenThuoc}</td>
                <td>${x.donViTinh}</td>
                <td>${x.soLuong.toLocaleString("vi-VN")}</td>
                <td>${x.donGiaNhap.toLocaleString("vi-VN")}</td>
                <td>${x.thanhTien.toLocaleString("vi-VN")}</td>
            `;
                tbody.appendChild(tr);
            });
        }

        const tong = detail.tienPhaiTra ?? detail.tongTienHang;
        const daTra = detail.daThanhToan || 0;
        const conNo = detail.conNo || 0;

        modal.querySelector(".detail-summary div:nth-child(1) strong").textContent =
            tong.toLocaleString("vi-VN") + " đ";
        modal.querySelector(".label-da-thanh-toan-ncc").textContent =
            daTra.toLocaleString("vi-VN") + " đ";
        modal.querySelector(".label-con-no-ncc").textContent =
            conNo.toLocaleString("vi-VN") + " đ";

        modal.classList.add("open");
    };
    // dashboard

    App.loadDashboard = function () {
        if (window.chrome && window.chrome.webview) {
            window.chrome.webview.postMessage({
                action: "getDashboardSummary",
                data: {}
            });
        }
    };
    App.loadDashboard();
    function formatDateTime(dt) {
        if (!dt) return "";
        // nếu từ C# trả về dạng ISO string
        const d = new Date(dt);
        if (isNaN(d.getTime())) return "";
        return d.toLocaleString("vi-VN");
    }

    App.onDashboardSummary = function (res) {
        if (!res) return;

        const rev = document.getElementById("dash-today-revenue");
        const orders = document.getElementById("dash-today-orders");
        const pending = document.getElementById("dash-pending-orders");
        const lowStock = document.getElementById("dash-lowstock-count");
        const tbody = document.getElementById("dash-activity-body");

        if (rev) {
            const v = res.revenueToday || 0;
            rev.textContent = v.toLocaleString("vi-VN") + " đ";
        }
        if (orders) {
            const c = res.ordersToday || 0;
            orders.textContent = c + " đơn thuốc";
        }
        if (pending) {
            pending.textContent = (res.pendingOrders || 0).toString();
        }
        if (lowStock) {
            lowStock.textContent = (res.lowStockCount || 0).toString();
        }

        if (tbody) {
            tbody.innerHTML = "";
            (res.recentActivities || []).forEach(act => {
                const tr = document.createElement("tr");
                tr.innerHTML = `
                <td>${formatDateTime(act.time)}</td>
                <td>${act.type || ""}</td>
                <td>${act.description || ""}</td>
                <td>${act.actor || ""}</td>
            `;
                tbody.appendChild(tr);
            });
        }
    };
    const btnDashExport = document.getElementById("btn-dashboard-export");
    if (btnDashExport) {
        btnDashExport.onclick = () => {
            if (window.chrome && window.chrome.webview) {
                window.chrome.webview.postMessage({
                    action: "exportDashboardReport",
                    data: {}
                });
            } else {
                alert("Demo xuất báo cáo – chỉ chạy trong WinApp.");
            }
        };
    }

    App.onExportDashboardReportResult = function (res) {
        if (!res) return;
        if (!res.success) {
            alert(res.message || "Xuất báo cáo tổng quan thất bại.");
        }
        // success: PDF đã tự mở rồi, không cần làm gì thêm
    };
    
    // bao cao
    App.loadBaoCaoThang = function (year, month) {
        if (window.chrome && window.chrome.webview) {
            window.chrome.webview.postMessage({
                action: "getBaoCaoThang",
                data: { year: year, month: month }
            });
        }
    };

    function parseMonthInput(value) {
        // value dạng "2025-11"
        if (!value) return null;
        const parts = value.split("-");
        if (parts.length !== 2) return null;
        const y = parseInt(parts[0], 10);
        const m = parseInt(parts[1], 10);
        if (!y || !m) return null;
        return { year: y, month: m };
    }
    App.showPage = function (pageId) {
        document.querySelectorAll(".page").forEach(p => {
            p.classList.toggle("active", p.id === pageId);
        });

        if (pageId === "page-baocao") {
            const input = document.getElementById("bc-month");
            const now = new Date();
            if (input && !input.value) {
                const m = (now.getMonth() + 1).toString().padStart(2, "0");
                input.value = `${now.getFullYear()}-${m}`;
            }
            const parsed = parseMonthInput(input.value);
            if (parsed) {
                App.loadBaoCaoThang(parsed.year, parsed.month);
            }
        }
    };
    document.getElementById("bc-btn-refresh")?.addEventListener("click", () => {
        const input = document.getElementById("bc-month");
        const parsed = parseMonthInput(input?.value || "");
        if (!parsed) {
            alert("Vui lòng chọn tháng hợp lệ.");
            return;
        }
        App.loadBaoCaoThang(parsed.year, parsed.month);
    });
    function formatMoney(v) {
        const n = Number(v || 0);
        return n.toLocaleString("vi-VN") + " đ";
    }

    function formatDate(d) {
        if (!d) return "";
        const dt = new Date(d);
        if (isNaN(dt.getTime())) return "";
        return dt.toLocaleDateString("vi-VN");
    }

    App.onBaoCaoThangResult = function (res) {
        if (!res) return;

        // cards
        document.getElementById("bc-today-revenue").textContent = formatMoney(res.doanhThuHomNay);
        document.getElementById("bc-today-orders").textContent =
            (res.soDonHomNay || 0) + " đơn thuốc";

        document.getElementById("bc-month-revenue").textContent = formatMoney(res.doanhThuThang);
        const revThis = Number(res.doanhThuThang || 0);
        const revPrev = Number(res.doanhThuThangTruoc || 0);
        let changeText = "So với tháng trước: 0%";
        if (revPrev > 0) {
            const pct = ((revThis - revPrev) / revPrev) * 100;
            changeText = "So với tháng trước: " + pct.toFixed(1) + "%";
        }
        document.getElementById("bc-month-revenue-change").textContent = changeText;

        document.getElementById("bc-month-import").textContent = formatMoney(res.giaTriNhapThang);
        document.getElementById("bc-month-import-count").textContent =
            (res.soPhieuNhapThang || 0) + " phiếu nhập";

        // 7 ngày gần nhất
        const tbody7 = document.getElementById("bc-7days-body");
        if (tbody7) {
            tbody7.innerHTML = "";
            (res.doanhThu7Ngay || []).forEach(item => {
                const tr = document.createElement("tr");
                tr.innerHTML = `
                <td>${formatDate(item.ngay)}</td>
                <td>${item.soDon || 0}</td>
                <td>${formatMoney(item.doanhThu)}</td>
            `;
                tbody7.appendChild(tr);
            });
        }

        // top thuốc
        const tbTop = document.getElementById("bc-topthuoc-body");
        if (tbTop) {
            tbTop.innerHTML = "";
            (res.topThuoc || []).forEach((item, idx) => {
                const tr = document.createElement("tr");
                tr.innerHTML = `
                <td>${idx + 1}</td>
                <td>${item.tenThuoc || ""}</td>
                <td>${item.donViTinh || ""}</td>
                <td>${Number(item.soLuongBan || 0).toLocaleString("vi-VN")}</td>
                <td>${formatMoney(item.doanhThu)}</td>
            `;
                tbTop.appendChild(tr);
            });
        }

        // công nợ tổng
        const tongAr = Number(res.tongPhaiThu || 0);
        const tongAp = Number(res.tongPhaiTra || 0);
        document.getElementById("bc-total-ar").textContent = formatMoney(tongAr);
        document.getElementById("bc-total-ar-count").textContent =
            (res.soKhachConNo || 0) + " khách hàng còn nợ";

        document.getElementById("bc-total-ap").textContent = formatMoney(tongAp);
        document.getElementById("bc-total-ap-count").textContent =
            (res.soNccConNo || 0) + " nhà cung cấp";

        const balance = tongAr - tongAp;
        document.getElementById("bc-total-balance").textContent = formatMoney(balance);
        const note = document.getElementById("bc-total-balance-note");
        if (note) {
            if (balance >= 0) {
                note.textContent = "Dương nghĩa là phải thu nhiều hơn phải trả.";
            } else {
                note.textContent = "Âm nghĩa là tiền phải trả lớn hơn phải thu.";
            }
        }

        // bảng công nợ KH
        const tbKh = document.getElementById("bc-congno-kh-body");
        if (tbKh) {
            tbKh.innerHTML = "";
            (res.congNoKh || []).forEach(item => {
                const statusHtml = (() => {
                    if (!item.hanThanhToanGanNhat) return '<span class="badge badge-outline">Không hạn</span>';
                    const d = new Date(item.hanThanhToanGanNhat);
                    const today = new Date();
                    const diff = (d - today) / (1000 * 60 * 60 * 24);
                    if (diff < 0) return '<span class="badge badge-outline">Quá hạn</span>';
                    if (diff <= 3) return '<span class="badge badge-warning">Sắp đến hạn</span>';
                    return '<span class="badge badge-success">Trong hạn</span>';
                })();

                const tr = document.createElement("tr");
                tr.innerHTML = `
                <td>${item.maKhachHang || ""}</td>
                <td>${item.hoTen || ""}</td>
                <td>${item.soDonConNo || 0}</td>
                <td>${formatMoney(item.tongNo)}</td>
                <td>${formatDate(item.hanThanhToanGanNhat)}</td>
                <td>${statusHtml}</td>
            `;
                tbKh.appendChild(tr);
            });
        }

        // bảng công nợ NCC
        const tbNcc = document.getElementById("bc-congno-ncc-body");
        if (tbNcc) {
            tbNcc.innerHTML = "";
            (res.congNoNcc || []).forEach(item => {
                const statusHtml = (() => {
                    if (!item.hanThanhToanGanNhat) return '<span class="badge badge-outline">Không hạn</span>';
                    const d = new Date(item.hanThanhToanGanNhat);
                    const today = new Date();
                    const diff = (d - today) / (1000 * 60 * 60 * 24);
                    if (diff < 0) return '<span class="badge badge-outline">Quá hạn</span>';
                    if (diff <= 3) return '<span class="badge badge-warning">Sắp đến hạn</span>';
                    return '<span class="badge badge-success">Trong hạn</span>';
                })();

                const tr = document.createElement("tr");
                tr.innerHTML = `
                <td>${item.tenNcc || ""}</td>
                <td>${item.soPhieuConNo || 0}</td>
                <td>${formatMoney(item.tongNo)}</td>
                <td>${formatDate(item.hanThanhToanGanNhat)}</td>
                <td>${statusHtml}</td>
            `;
                tbNcc.appendChild(tr);
            });
        }
    };
    function getSelectedReportMonth() {
        const input = document.getElementById("bc-month");
        const parsed = parseMonthInput(input?.value || "");
        if (!parsed) {
            alert("Vui lòng chọn tháng hợp lệ.");
            return null;
        }
        return parsed;
    }

    document.getElementById("bc-btn-export-stock")?.addEventListener("click", () => {
        const m = getSelectedReportMonth();
        if (!m) return;
        window.chrome?.webview?.postMessage({
            action: "exportBaoCaoKho",
            data: { year: m.year, month: m.month }
        });
    });

    document.getElementById("bc-btn-export-debt")?.addEventListener("click", () => {
        const m = getSelectedReportMonth();
        if (!m) return;
        window.chrome?.webview?.postMessage({
            action: "exportBaoCaoCongNo",
            data: { year: m.year, month: m.month }
        });
    });

    document.getElementById("bc-btn-export-summary")?.addEventListener("click", () => {
        const m = getSelectedReportMonth();
        if (!m) return;
        window.chrome?.webview?.postMessage({
            action: "exportBaoCaoTongHop",
            data: { year: m.year, month: m.month }
        });
    });

    App.onExportBaoCaoResult = function (res) {
        if (!res) return;
        if (!res.success) {
            alert("Xuất " + (res.type || "báo cáo") + " thất bại.");
        }
        // success: PDF đã tự mở bên Windows nên không cần làm thêm
    };
    function getUserSearchFilter() {
        const txt = document.getElementById("account-user-search");
        const sel = document.getElementById("account-user-filter");
        const text = (txt?.value || "").trim().toLowerCase();
        const status = sel?.value || "all";
        return { text, status };
    }

    function filterUserList(list) {
        const { text, status } = getUserSearchFilter();

        return (list || []).filter(u => {
            // lọc theo text
            if (text) {
                const userName = (u.userName || "").toLowerCase();
                const fullName = (u.fullName || "").toLowerCase();
                const email = (u.email || "").toLowerCase();

                if (!userName.includes(text) &&
                    !fullName.includes(text) &&
                    !email.includes(text)) {
                    return false;
                }
            }

            // lọc theo trạng thái
            const active = !!u.isActive;
            if (status === "active" && !active) return false;
            if (status === "inactive" && active) return false;

            return true;
        });
    }

    function renderUserTable() {
        const tbody = document.querySelector("#account-user-table tbody");
        const countEl = document.getElementById("account-user-count");
        if (!tbody) return;

        const items = filterUserList(App.userList || []);

        tbody.innerHTML = "";

        items.forEach(u => {
            const tr = document.createElement("tr");
            tr.dataset.userId = u.userId;

            const active = !!u.isActive;
            const statusText = active ? "Đang hoạt động" : "Đã khóa";
            const statusClass = active ? "badge-success" : "badge-warning";
            if (!active) tr.classList.add("account-user-row--inactive");

            tr.innerHTML = `
            <td>${u.userName || ""}</td>
            <td>${u.fullName || ""}</td>
            <td>${u.roleName || ""}</td>
            <td>${u.email || ""}</td>
            <td><span class="badge ${statusClass}">${statusText}</span></td>
            <td>
                <button class="btn btn-sm btn-outline btn-user-select" type="button">
                    Chọn
                </button>
            </td>
        `;
            tbody.appendChild(tr);
        });

        if (countEl) {
            countEl.textContent = `${items.length} tài khoản`;
        }
    }

    function collectKhachHangForm() {
        const ten = document.getElementById("khach-ten")?.value.trim() || "";
        const sdt = document.getElementById("khach-sdt")?.value.trim() || "";
        const diaChi = document.getElementById("khach-diachi")?.value.trim() || "";
        const ghiChu = document.getElementById("khach-ghichu")?.value.trim() || "";

        if (!ten) {
            alert("Vui lòng nhập tên khách hàng");
            return null;
        }

        return {
            // KHÔNG gửi maKhachHang, backend tự sinh
            hoTen: ten,
            soDienThoai: sdt,
            diaChi: diaChi,
            ghiChu: ghiChu
        };
    }
    function initKhachSearch() {
        const input = document.getElementById("khach-search");
        if (!input) return;
        input.addEventListener("input", () => {
            renderKhachTable();
        });
    }
    function initThuocSearch() {
        const input = document.getElementById("thuoc-search");
        if (!input) return;
        input.addEventListener("input", () => {
            renderThuocTable();
        });
    }
    function initUserSearch() {
        const txt = document.getElementById("account-user-search");
        const sel = document.getElementById("account-user-filter");

        if (txt) {
            txt.addEventListener("input", () => {
                renderUserTable();
            });
        }

        if (sel) {
            sel.addEventListener("change", () => {
                renderUserTable();
            });
        }
    }
    initUserSearch();
    initThuocSearch();
    initKhachSearch();
    requestPhieuNhapList();
    bindDonThuocRowEvents();
    // cuối file
    bindThuocRowEvents();
    initNhapKhoModal(); 
    const bcInput = document.getElementById("bc-month");
    if (bcInput) {
        const now = new Date();
        const m = (now.getMonth() + 1).toString().padStart(2, "0");
        bcInput.value = `${now.getFullYear()}-${m}`;

        const parsed = parseMonthInput(bcInput.value);
        if (parsed) {
            App.loadBaoCaoThang(parsed.year, parsed.month);
        }
    }
});
