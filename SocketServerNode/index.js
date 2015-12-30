// Load the TCP Library
var net = require('net');
var app = require('express')();
var http = require('http').Server(app);

// Keep track of the chat clients
var clients = [];
var theSocket;

// Start a TCP Server
net.createServer(function (socket) {
	
	theSocket = socket;
	
	// Identify this client
	socket.name = socket.remoteAddress + ":" + socket.remotePort
	
	// Put this new client in the list
	clients.push(socket);
	
	// Send a nice welcome message and announce
	socket.write("Welcome " + socket.name + "\n");
	
	// Handle incoming messages from clients.
	socket.on('data', function (data) {
		console.log(socket.name + "> " + data);
	});
	
	// Remove the client from the list when it leaves
	socket.on('end', function () {
		clients.splice(clients.indexOf(socket), 1);
		console.log(socket.name + " left the chat.\n");
	});
}).listen(1983);

http.listen(3000, function () {
	console.log("listening http on 3000");
})

app.get('/send', function (req, res) {
	theSocket.write("This is a sample message");
	res.send("<h1>Message sent</h1>");
});

// Put a friendly message on the terminal of the server.
console.log("Chat server running at port 1983\n");
