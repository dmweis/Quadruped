let videoImage = document.getElementById('video_image');
videoImage.src = `http://${document.domain}:8080/?action=stream`;
let cameraJoystick = {
    zone: videoImage,
    color: 'red'
};
let cameraJoystickManager = nipplejs.create(cameraJoystick);
const clickTimeout = 500;
var lastStopClick = Date.now();
cameraJoystickManager.on('added',
    function (evt, nipple) {
        nipple.on('move',
            function (evt, arg) {
                var json = JSON.stringify({
                    joystickType: 'camera',
                    angle: arg.angle.radian,
                    MessageType: 'movement',
                    distance: arg.distance
                });
                serverSocket.send(json);
            });
        nipple.on('start',
            function () {
                const json = JSON.stringify(
                    { joystickType: 'camera', MessageType: 'start' });
                serverSocket.send(json);
            });
        nipple.on('end',
            function () {
                if (Date.now() - lastStopClick < clickTimeout) {
                    const json = JSON.stringify(
                        { joystickType: 'camera', MessageType: 'reset' });
                    serverSocket.send(json);
                } else {
                    var json = JSON.stringify({
                        joystickType: 'camera',
                        MessageType: 'stop'
                    });
                    serverSocket.send(json);
                }
                lastStopClick = Date.now();
            });
    });