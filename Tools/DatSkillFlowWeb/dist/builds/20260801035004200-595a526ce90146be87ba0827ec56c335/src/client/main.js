// dat-skill-flow-build:20260801035004200-595a526ce90146be87ba0827ec56c335
                          
                
            
                       
                     
      
 

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
