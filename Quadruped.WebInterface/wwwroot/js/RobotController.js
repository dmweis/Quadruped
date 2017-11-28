var uri = `ws://${document.domain}:${location.port}/ws`;
var serverSocket = new WebSocket(uri);
window.onbeforeunload = function () {
    serverSocket.close();
};

serverSocket.onmessage = function(event) {
    console.log(JSON.parse(event.data));
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
