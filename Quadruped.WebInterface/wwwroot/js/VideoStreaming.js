let videoImage = document.getElementById('video_image');
videoImage.src = `http://${document.domain}:8080/?action=stream`;


const clickTimeout = 500;
var lastStopClick = Date.now();

const cameraJoystick = createJoystick({
    elementId: "#video_image_container",
    color: "red",
    name: "camera",
    socket: serverSocket,
    startCallback: function () {
        if (Date.now() - lastStopClick < clickTimeout) {
            const json = JSON.stringify(
                { joystickType: 'camera', MessageType: 'reset' });
            serverSocket.send(json);
        }
        lastStopClick = Date.now();
    }
});
