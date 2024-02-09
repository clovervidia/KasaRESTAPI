let updateInterval = null;

google.charts.load("current", {
    packages: ["corechart", "line"]
});

const darkBackgroundColor = "#212529";
const darkBodyTextColor = "#dee2e6";

const ids = {
    placeholder: "placeholder",
    deviceList: "device-list",
    selectedDevice: "selected-device",
    currentChart: "current-chart-div",
    voltageChart: "voltage-chart-div",
    powerChart: "power-chart-div",
};

function getDevices() {
    fetch("api/devices").then(response => response.json()).then(data => displayDevices(data)).catch(error => console.error(`Unable to get devices: ${error}`));
}

function displayDevices(data) {
    const placeholder = document.getElementById(ids.placeholder);
    placeholder.innerText = "If you don't see your device below, wait a few seconds and refresh.";

    const bulletList = document.createElement("ul");
    bulletList.id = "device-list";
    for (let p of data) {
        let bullet = document.createElement("li");
        let link = document.createElement("a");
        link.addEventListener("click", () => displayEnergyStats(p.macAddress));
        link.href = "#";
        link.innerText = `${p.macAddress} (${p.name})`;
        bullet.appendChild(link);
        bulletList.appendChild(bullet);
    }
    placeholder.parentNode.insertBefore(bulletList, placeholder.nextSibling);
}

function displayEnergyStats(mac) {
    const deviceList = document.getElementById(ids.deviceList);
    const selectedDevice = document.createElement("h3");
    selectedDevice.id = "selected-device";
    selectedDevice.innerText = mac;

    const existingSelectedDevice = document.getElementById(ids.selectedDevice);
    if (existingSelectedDevice) {
        existingSelectedDevice.parentNode.removeChild(existingSelectedDevice);
    }

    deviceList.parentNode.insertBefore(selectedDevice, deviceList.nextSibling);

    const chartsDiv = document.createElement("div");
    selectedDevice.parentNode.insertBefore(chartsDiv, deviceList.selectedDevice);

    for (let c of [ids.currentChart, ids.voltageChart, ids.powerChart]) {
        let existingChart = document.getElementById(c);
        if (existingChart) {
            existingChart.parentNode.removeChild(existingChart);
        }
        let newChart = document.createElement("div");
        newChart.id = c;
        chartsDiv.insertBefore(newChart, null);
    }

    const currentData = new google.visualization.DataTable();
    currentData.addColumn("datetime", "X");
    currentData.addColumn("number", "Current");

    const voltageData = new google.visualization.DataTable();
    voltageData.addColumn("datetime", "X");
    voltageData.addColumn("number", "Voltage");

    const powerData = new google.visualization.DataTable();
    powerData.addColumn("datetime", "X");
    powerData.addColumn("number", "Power");

    let now = new Date();
    for (let i = 0; i < 30; i++) {
        let temp = new Date(now);
        temp.setSeconds(temp.getSeconds() - ((30 - i) * 3));
        currentData.addRow([temp, 0]);
        voltageData.addRow([temp, 0]);
        powerData.addRow([temp, 0]);
    }

    const currentOptions = {
        hAxis: {
            textStyle: {
                color: darkBodyTextColor
            }
        },
        vAxis: {
            title: "Amps",
            viewWindow: {
                min: 0
            },
            textStyle: {
                color: darkBodyTextColor
            },
            titleTextStyle: {
                color: darkBodyTextColor
            }
        },
        legend: {
            textStyle: {
                color: darkBodyTextColor
            }
        },
        backgroundColor: darkBackgroundColor,
        title: "Current",
        titleTextStyle: {
            color: darkBodyTextColor
        }
    };
    const currentChart = new google.visualization.LineChart(document.getElementById(ids.currentChart));
    currentChart.draw(currentData, currentOptions);

    const voltageOptions = {
        hAxis: {
            textStyle: {
                color: darkBodyTextColor
            }
        },
        vAxis: {
            title: "Volts",
            viewWindow: {
                min: 0
            },
            textStyle: {
                color: darkBodyTextColor
            },
            titleTextStyle: {
                color: darkBodyTextColor
            }
        },
        legend: {
            textStyle: {
                color: darkBodyTextColor
            }
        },
        backgroundColor: darkBackgroundColor,
        title: "Voltage",
        titleTextStyle: {
            color: darkBodyTextColor
        }
    };
    const voltageChart = new google.visualization.LineChart(document.getElementById(ids.voltageChart));
    voltageChart.draw(voltageData, voltageOptions);

    const powerOptions = {
        hAxis: {
            textStyle: {
                color: darkBodyTextColor
            }
        },
        vAxis: {
            title: "Watts",
            viewWindow: {
                min: 0
            },
            textStyle: {
                color: darkBodyTextColor
            },
            titleTextStyle: {
                color: darkBodyTextColor
            }
        },
        legend: {
            textStyle: {
                color: darkBodyTextColor
            }
        },
        backgroundColor: darkBackgroundColor,
        title: "Power",
        titleTextStyle: {
            color: darkBodyTextColor
        }
    };
    const powerChart = new google.visualization.LineChart(document.getElementById(ids.powerChart));
    powerChart.draw(powerData, powerOptions);

    function getStats() {
        fetch(`api/${mac}/energymonitor`).then(response => response.json()).then(data => updateChart(data)).catch(error => console.error(`Unable to get energy monitoring stats: ${error}`));
    }

    if (updateInterval) {
        clearInterval(updateInterval);
        updateInterval = null;
    }

    getStats();
    updateInterval = setInterval(getStats, 3000);

    let x = 30;

    function updateChart(stats) {
        now = new Date();
        currentData.addRow([now, stats.current]);
        currentData.removeRow(0)
        currentChart.draw(currentData, currentOptions);

        voltageData.addRow([now, stats.voltage]);
        voltageData.removeRow(0)
        voltageChart.draw(voltageData, voltageOptions);

        powerData.addRow([now, stats.power]);
        powerData.removeRow(0)
        powerChart.draw(powerData, powerOptions);

        selectedDevice.innerText = `${mac} (${stats.total} kWh)`;
    }
}

getDevices();
