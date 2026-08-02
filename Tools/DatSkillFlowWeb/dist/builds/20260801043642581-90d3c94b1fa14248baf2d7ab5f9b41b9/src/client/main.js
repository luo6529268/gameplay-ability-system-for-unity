// dat-skill-flow-build:20260801043642581-90d3c94b1fa14248baf2d7ab5f9b41b9
                          
                
            
                       
                     
      
 

const statusElement = document.querySelector             ("#server-status");
const diagnosticsElement = document.querySelector             ("#diagnostics");

async function connect()                {
    if (statusElement === null || diagnosticsElement === null) {
        return;
    }

    try {
        const response = await fetch("/api/health", {
            headers: { Accept: "application/json" },
        });
        const health = await response.json()                  ;
        if (!response.ok || !health.ok || health.data?.host !== "127.0.0.1") {
            throw new Error("Unexpected health response");
        }
        statusElement.textContent = "Connected to local server";
        statusElement.dataset.state = "connected";
    } catch {
        statusElement.textContent = "Local server unavailable";
        statusElement.dataset.state = "error";
        diagnosticsElement.textContent = "The loopback health check failed. No data was changed.";
    }
}

void connect();
