async function loadSwaggerToken() {
    const localToken = window.localStorage.getItem("CADSFINANCE_SWAGGER_TOKEN");
    if (localToken) {
        return localToken;
    }

    const response = await fetch("/swagger-auth-token", {
        credentials: "same-origin",
        headers: {
            "Accept": "application/json"
        }
    });

    if (!response.ok) {
        return null;
    }

    const payload = await response.json();
    if (!payload || !payload.token) {
        return null;
    }

    window.localStorage.setItem("CADSFINANCE_SWAGGER_TOKEN", payload.token);
    return payload.token;
}

function waitForSwaggerUi(callback) {
    if (window.ui && typeof window.ui.preauthorizeApiKey === "function") {
        callback();
        return;
    }

    window.setTimeout(function () {
        waitForSwaggerUi(callback);
    }, 100);
}

(function () {
    fetch("/swagger-auth-token", {
        credentials: "same-origin",
        headers: {
            "Accept": "application/json"
        }
    })
        .then(function (response) {
            if (!response.ok) {
                return null;
            }

            return response.json();
        })
        .then(function (payload) {
            if (payload && payload.token) {
                window.localStorage.setItem("CADSFINANCE_SWAGGER_TOKEN", payload.token);
            }
        })
        .catch(function () {
            window.localStorage.removeItem("CADSFINANCE_SWAGGER_TOKEN");
        });

    waitForSwaggerUi(async function () {
        try {
            const token = await loadSwaggerToken();
            if (token) {
                window.ui.preauthorizeApiKey("Bearer", token);
            }
        } catch {
            window.localStorage.removeItem("CADSFINANCE_SWAGGER_TOKEN");
        }
    });
})();
