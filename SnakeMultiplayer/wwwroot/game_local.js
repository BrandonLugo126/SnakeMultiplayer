const BOARD_WIDTH = 20;
const BOARD_HEIGHT = 15;
const CELL_SIZE = 30;

let gameRunning = false;
let gameLoop = null;
let snake1 = [];
let snake2 = [];
let food = { x: 0, y: 0 };
let direction1 = "RIGHT";
let direction2 = "LEFT";
let nextDirection1 = "RIGHT";
let nextDirection2 = "LEFT";
let score1 = 0;
let score2 = 0;
let player1Name = "";
let player2Name = "";
let canvas = null;
let ctx = null;

document.getElementById("startBtn").addEventListener("click", startGame);
document.getElementById("backBtn").addEventListener("click", backToMenu);
document.addEventListener("keydown", handleKeyPress);

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/viborahub")
    .configureLogging(signalR.LogLevel.Information)
    .withAutomaticReconnect()
    .build();

async function start() {
    try {
        await connection.start();
        console.log("SignalR Connected.");
    } catch (err) {
        console.log(err);
        setTimeout(start, 5000);
    }
};

connection.onclose(async () => {
    await start();
});

start();

connection.on("EsperandoConexion", () => {
    let bottom = document.getElementById("startBtn");
    let mensaje = document.createElement("h2");
    mensaje.classList.add("message");
    mensaje.textContent = "Esperando conexión...";
    document.getElementById("player1Name").disabled=true;

    bottom.parentNode.append(mensaje);

    bottom.style.display = "none";
});

connection.on("JuegoIniciado", (nombreJugador2,tablero) => {
    document.getElementById("menu").style.display = "none";
    document.getElementById("game").style.display = "block";
    
    player2Name = nombreJugador2;

    document.getElementById("score1").textContent = `${player1Name}: 0`;
    document.getElementById("score2").textContent = `${player2Name}: 0`;

    canvas = document.getElementById("gameCanvas");
    ctx = canvas.getContext("2d");
    drawGame(tablero);
    gameRunning = true;

});

connection.on("TableroActualizado", (tablero) => {
    drawGame(tablero);
});

async function  startGame() {
    player1Name = document.getElementById("player1Name").value.trim();
    await connection.invoke("conectar", player1Name);

    if (!player1Name) player1Name = "Jugador";  

    // document.getElementById("menu").style.display = "none";
    // document.getElementById("game").style.display = "block";



    // initGame();
    // gameLoop = setInterval(updateGame, 150);
    // 
    //
}

function backToMenu() {
    if (gameLoop) {
        clearInterval(gameLoop);
        gameLoop = null;
    }
    gameRunning = false;
    document.getElementById("menu").style.display = "block";
    document.getElementById("game").style.display = "none";
    document.getElementById("message").innerHTML = "";
}

function initGame() {
    snake1 = [
        { x: 5, y: 6 },
        { x: 4, y: 6 },
        { x: 3, y: 6 }
    ];

    snake2 = [
        { x: 14, y: 7 },
        { x: 15, y: 7 },
        { x: 16, y: 7 }
    ];

    direction1 = "RIGHT";
    direction2 = "LEFT";
    nextDirection1 = "RIGHT";
    nextDirection2 = "LEFT";
    score1 = 0;
    score2 = 0;

    updateScoreDisplay();
    generateRandomFood();
    drawGame();
}

function generateRandomFood() {
    const maxAttempts = 1000;

    for (let attempt = 0; attempt < maxAttempts; attempt++) {
        const candidate = {
            x: Math.floor(Math.random() * BOARD_WIDTH),
            y: Math.floor(Math.random() * BOARD_HEIGHT)
        };

        if (!snake1.some(s => s.x === candidate.x && s.y === candidate.y) &&
            !snake2.some(s => s.x === candidate.x && s.y === candidate.y)) {
            food = candidate;
            return;
        }
    }

    for (let y = 0; y < BOARD_HEIGHT; y++) {
        for (let x = 0; x < BOARD_WIDTH; x++) {
            if (!snake1.some(s => s.x === x && s.y === y) &&
                !snake2.some(s => s.x === x && s.y === y)) {
                food = { x, y };
                return;
            }
        }
    }
}

function updateGame() {
    if (!gameRunning) return;

    direction1 = nextDirection1;
    direction2 = nextDirection2;

    moveSnake(snake1, direction1, true);
    moveSnake(snake2, direction2, false);

    if (!gameRunning) return;

    if (checkFoodCollision(snake1, food)) {
        score1++;
        updateScoreDisplay();
        growSnake(snake1);
        generateRandomFood();
    }

    if (checkFoodCollision(snake2, food)) {
        score2++;
        updateScoreDisplay();
        growSnake(snake2);
        generateRandomFood();
    }

    drawGame();
}

function moveSnake(snake, direction, isPlayer1) {
    const newHead = { ...snake[0] };

    switch (direction) {
        case "UP": newHead.y--; break;
        case "DOWN": newHead.y++; break;
        case "LEFT": newHead.x--; break;
        case "RIGHT": newHead.x++; break;
    }

    const hitWall = newHead.x < 0 || newHead.x >= BOARD_WIDTH || newHead.y < 0 || newHead.y >= BOARD_HEIGHT;
    const hitSelf = snake.some(s => s.x === newHead.x && s.y === newHead.y);
    const otherSnake = isPlayer1 ? snake2 : snake1;
    const hitOther = otherSnake.some(s => s.x === newHead.x && s.y === newHead.y);

    if (hitWall || hitSelf || hitOther) {
        endGame(isPlayer1 ? player1Name : player2Name);
        return;
    }

    snake.unshift(newHead);
    snake.pop();
}

function checkFoodCollision(snake, food) {
    return snake[0].x === food.x && snake[0].y === food.y;
}

function growSnake(snake) {
    const tail = snake[snake.length - 1];
    const secondTail = snake[snake.length - 2];
    const newSegment = {
        x: tail.x + (tail.x - secondTail.x),
        y: tail.y + (tail.y - secondTail.y)
    };
    snake.push(newSegment);
}

function endGame(loserName) {
    gameRunning = false;
    if (gameLoop) {
        clearInterval(gameLoop);
        gameLoop = null;
    }

    const winner = loserName === player1Name ? player2Name : player1Name;
    document.getElementById("message").innerHTML = `${loserName} perdió! Ganador: ${winner}`;

    setTimeout(() => {
        backToMenu();
    }, 3000);
}

function updateScoreDisplay() {
    document.getElementById("score1").innerHTML = `${player1Name}: ${score1}`;
    document.getElementById("score2").innerHTML = `${player2Name}: ${score2}`;
}

function drawGame(tablero) {
    ctx.fillStyle = "#111";
    ctx.fillRect(0, 0, canvas.width, canvas.height);

    ctx.strokeStyle = "#333";
    for (let i = 0; i <= BOARD_WIDTH; i++) {
        ctx.beginPath();
        ctx.moveTo(i * CELL_SIZE, 0);
        ctx.lineTo(i * CELL_SIZE, canvas.height);
        ctx.stroke();
        ctx.moveTo(0, i * CELL_SIZE);
        ctx.lineTo(canvas.width, i * CELL_SIZE);
        ctx.stroke();
    }

    tablero.serpiente1.forEach((segment, index) => {
        ctx.fillStyle = index === 0 ? "#4CAF50" : "#81C784";
        ctx.fillRect(segment.x * CELL_SIZE, segment.y * CELL_SIZE, CELL_SIZE - 1, CELL_SIZE - 1);
    });

    tablero.serpiente2.forEach((segment, index) => {
        ctx.fillStyle = index === 0 ? "#FF5722" : "#FF8A65";
        ctx.fillRect(segment.x * CELL_SIZE, segment.y * CELL_SIZE, CELL_SIZE - 1, CELL_SIZE - 1);
    });

    ctx.fillStyle = "#FFD700";
    ctx.fillRect(tablero.manzana.x * CELL_SIZE, tablero.manzana.y * CELL_SIZE, CELL_SIZE - 1, CELL_SIZE - 1);
}

async function handleKeyPress(e) {
    if (!gameRunning) return;

    const key = e.key;


    if (key === "ArrowUp") {
        await connection.invoke("Mover",player1Name,"Arriba")
    } else if (key === "ArrowDown") {
        await connection.invoke("Mover", player1Name, "Abajo")
    } else if (key === "ArrowLeft") {
        await connection.invoke("Mover", player1Name, "Izquierda")
    } else if (key === "ArrowRight") {
        await connection.invoke("Mover", player1Name, "Derecha")
    }
}