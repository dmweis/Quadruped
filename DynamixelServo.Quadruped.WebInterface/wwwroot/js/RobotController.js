var uri = `ws://${document.domain}:${location.port}/ws`;
var serverSocket = new WebSocket(uri);
let moveJoystickZone = document.getElementById('move_joystick_zone');
var moveJoystickTextField = document.getElementById('move_joystick_text_field');
let rotationJoystickZone = document.getElementById('rotate_joystick_zone');
var rotationJoystickTextField = document.getElementById('rotation_joystick_text_field');
let moveJoystickOptions = {
    zone: moveJoystickZone,
    color: 'red'
};
let rotationJoystickOptions = {
    zone: rotationJoystickZone,
    color: 'navy'
};
let moveJoystickManager = nipplejs.create(moveJoystickOptions);
let rotationJoystickManager = nipplejs.create(rotationJoystickOptions);
moveJoystickManager.on('added',
    function (evt, nipple) {
        nipple.on('move',
            function (evt, arg) {
                var json = JSON.stringify({
                    joystickType: 'direction',
                    angle: arg.angle.radian,
                    MessageType: 'movement',
                    distance: arg.distance
                });
                serverSocket.send(json);
                //moveJoystickTextField.innerText = json;
            });
        nipple.on('start',
            function (evt) {
                var json = JSON.stringify(
                    { joystickType: 'direction', MessageType: 'start' });
                serverSocket.send(json);
            });
        nipple.on('end',
            function (evt) {
                var json = JSON.stringify({
                    joystickType: 'direction',
                    MessageType: 'stop'
                });
                serverSocket.send(json);
            });
    });
rotationJoystickManager.on('added',
    function (evt, nipple) {
        nipple.on('move',
            function (evt, arg) {
                var json = JSON.stringify({
                    joystickType: 'rotation',
                    angle: arg.angle.radian,
                    MessageType: 'movement',
                    distance: arg.distance
                });
                serverSocket.send(json);
                //rotationJoystickTextField.innerText = json;
            });
        nipple.on('start',
            function (evt) {
                var json = JSON.stringify({
                    joystickType: 'rotation',
                    MessageType: 'start'
                });
                serverSocket.send(json);
            });
        nipple.on('end',
            function (evt) {
                var json = JSON.stringify({ joystickType: 'rotation', MessageType: 'stop' });
                serverSocket.send(json);
            });
    });