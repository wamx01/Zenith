window.mundoVsAuth = {
    request: async function (url, payload) {
        const response = await fetch(url, {
            method: 'POST',
            credentials: 'same-origin',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            },
            body: payload ? JSON.stringify(payload) : null
        });

        let data = null;
        const contentType = response.headers.get('content-type') || '';
        if (contentType.includes('application/json')) {
            data = await response.json();
        }

        return {
            ok: response.ok,
            status: response.status,
            data: data
        };
    },
    login: async function (url, payload) {
        const result = await this.request(url, payload);
        return {
            succeeded: result.ok && !!result.data?.succeeded,
            error: result.data?.error || null,
            redirectUrl: result.data?.redirectUrl || null
        };
    },
    refresh: async function (url) {
        const result = await this.request(url, {});
        return {
            succeeded: result.ok && !!result.data?.succeeded,
            error: result.data?.error || null,
            redirectUrl: result.data?.redirectUrl || null
        };
    },
    logout: async function (url) {
        await this.request(url, {});
    },
    downloadCsv: function (fileName, content) {
        try {
            const safeName = (fileName && String(fileName).trim()) || `export-${Date.now()}.csv`;
            const bom = '\uFEFF';
            const blob = new Blob([bom + (content ?? '')], { type: 'text/csv;charset=utf-8;' });
            const url = URL.createObjectURL(blob);
            const link = document.createElement('a');
            link.href = url;
            link.download = safeName;
            link.style.display = 'none';
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            setTimeout(() => URL.revokeObjectURL(url), 1000);
        } catch (err) {
            console.error('mundoVsAuth.downloadCsv failed', err);
            throw err;
        }
    }
};
