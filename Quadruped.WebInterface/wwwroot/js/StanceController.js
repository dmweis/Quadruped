var serverSocket = createServerConnection();

const telemtricsVm = new Vue({
    el: '#telemetrics_display',
    data: {
        averageTemperature: 0,
        averageVoltage: 0
    }
});

serverSocket.onmessage = function (event) {
    const networkMessage = JSON.parse(event.data);
    if (networkMessage.topic === "telemetrics") {
        telemtricsVm.averageTemperature = networkMessage.message.AverageTemperature;
        telemtricsVm.averageVoltage = networkMessage.message.AverageVoltage;
    }
};

const translationJoystick = createJoystick({
    elementId: "#translation_joystick_zone",
    color: "red",
    name: "translation",
    socket: serverSocket
});

const heightJoystick = createJoystick({
    elementId: "#height_joystick_zone",
    color: "navy",
    name: "height",
    socket: serverSocket
});

const bodyRotationJoystick = createJoystick({
    elementId: "#rotate_joystick_zone",
    color: "navy",
    name: "bodyRotation",
    socket: serverSocket
});
