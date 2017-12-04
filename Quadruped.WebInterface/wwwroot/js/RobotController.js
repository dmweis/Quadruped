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
    else if (networkMessage.topic === "IMU") {
        console.log(networkMessage.message.Gyroscopic);
    }
};

const movementJoystick = createJoystick({
    elementId: "#move_joystick_zone",
    color: "red",
    name: "direction",
    socket: serverSocket
});

const rotationJoystick = createJoystick({
    elementId: "#rotate_joystick_zone",
    color: "navy",
    name: "rotation",
    socket: serverSocket
});
