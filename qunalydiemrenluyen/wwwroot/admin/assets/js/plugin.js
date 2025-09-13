function changeTheme(mode) {
    let bootstrapStyle = document.getElementById("bootstrap-style");
    let appStyle = document.getElementById("app-style");

    switch (mode) {
        case "light-mode-switch":
            bootstrapStyle.setAttribute("href", "/admin/assets/css/bootstrap.min.css");
            appStyle.setAttribute("href", "/admin/assets/css/app.min.css");
            document.documentElement.setAttribute("data-bs-theme", "light");
            break;

        case "dark-mode-switch":
            bootstrapStyle.setAttribute("href", "/admin/assets/css/bootstrap.min.css"); // Có thể đổi nếu có file dark theme
            appStyle.setAttribute("href", "/admin/assets/css/app.min.css");
            document.documentElement.setAttribute("data-bs-theme", "dark");
            break;

        case "rtl-mode-switch":
            bootstrapStyle.setAttribute("href", "/admin/assets/css/bootstrap-rtl.min.css");
            appStyle.setAttribute("href", "/admin/assets/css/app-rtl.min.css");
            document.documentElement.setAttribute("dir", "rtl");
            document.documentElement.setAttribute("data-bs-theme", "light");
            break;
    }
}
