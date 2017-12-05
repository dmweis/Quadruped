import socket
import serial.tools.list_ports
import serial


ports = list(serial.tools.list_ports.comports())
arduino_port = next((port for port in ports if "Arduino" in port.description), None)

arduino = serial.Serial(arduino_port[0], 9600)

# print("Connecting on " + arduino_port[0])

PORT = 4242
HOST = 'localhost'

server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server_socket.bind((HOST, PORT))
server_socket.listen(5)

while True:
    connection, address = server_socket.accept()
    # print("Client connected")
    while True:
        try:
            incoming = arduino.readline()
            connection.send(incoming)
        except:
            # print("Client disconnected")
            break
