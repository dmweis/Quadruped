let videoImage = document.getElementById('video_image');
videoImage.src = `http://${document.domain}:8080/?action=stream`;
let cameraJoystick = {
    zone: videoImage,
    color: 'red'
};
let cameraJoystickManager = nipplejs.create(cameraJoystick);
const clickTimeout = 500;
var lastStartClick = getTime();
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
                if (lastStartClick - getTime() < clickTimeout) {
                    const json = JSON.stringify(
                        { joystickType: 'camera', MessageType: 'reset' });
                    serverSocket.send(json);
                } else {
                    const json = JSON.stringify(
                        { joystickType: 'camera', MessageType: 'start' });
                    serverSocket.send(json);
                }
                lastStartClick = getTime();
            });
        nipple.on('end',
            function () {
                var json = JSON.stringify({
                    joystickType: 'camera',
                    MessageType: 'stop'
                });
                serverSocket.send(json);
            });
    });