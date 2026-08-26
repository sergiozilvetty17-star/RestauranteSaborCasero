-- =====================================================
-- BASE DE DATOS
-- =====================================================

CREATE DATABASE restaurante_db;

USE restaurante_db;


-- =====================================================
-- TABLA: USUARIOS
-- =====================================================

CREATE TABLE usuarios (
    IdUsuario INT AUTO_INCREMENT PRIMARY KEY,
    Nombre VARCHAR(100) NOT NULL,
    Usuario VARCHAR(50) NOT NULL UNIQUE,
    Contraseña VARCHAR(255) NOT NULL,
    Rol VARCHAR(30) NOT NULL,
    Estado VARCHAR(20) NOT NULL DEFAULT 'Activo'
);


-- =====================================================
-- TABLA: MESAS
-- =====================================================

CREATE TABLE mesas (
    IdMesa INT AUTO_INCREMENT PRIMARY KEY,
    NumeroMesa INT NOT NULL UNIQUE,
    Estado VARCHAR(20) NOT NULL DEFAULT 'Disponible'
);


-- =====================================================
-- TABLA: PLATOS
-- =====================================================

CREATE TABLE platos (
    IdPlato INT AUTO_INCREMENT PRIMARY KEY,
    Nombre VARCHAR(100) NOT NULL,
    Precio DECIMAL(10,2) NOT NULL,
    Estado VARCHAR(20) NOT NULL DEFAULT 'Disponible',
    TiempoEstimado INT NOT NULL
);


-- =====================================================
-- TABLA: INGREDIENTES
-- =====================================================

CREATE TABLE ingredientes (
    IdIngrediente INT AUTO_INCREMENT PRIMARY KEY,
    Nombre VARCHAR(100) NOT NULL,
    CantidadDisponible DECIMAL(10,2) NOT NULL DEFAULT 0,
    CantidadMinima DECIMAL(10,2) NOT NULL DEFAULT 0,
    UnidadMedida VARCHAR(30) NOT NULL,
    Estado VARCHAR(20) NOT NULL DEFAULT 'Disponible'
);


-- =====================================================
-- TABLA: PLATO_INGREDIENTE
-- Relación N:M entre PLATOS e INGREDIENTES
-- =====================================================

CREATE TABLE plato_ingrediente (
    IdPlato INT NOT NULL,
    IdIngrediente INT NOT NULL,
    CantidadNecesaria DECIMAL(10,2) NOT NULL,

    PRIMARY KEY (IdPlato, IdIngrediente),

    CONSTRAINT fk_plato_ingrediente_plato
        FOREIGN KEY (IdPlato)
        REFERENCES platos(IdPlato)
        ON DELETE CASCADE
        ON UPDATE CASCADE,

    CONSTRAINT fk_plato_ingrediente_ingrediente
        FOREIGN KEY (IdIngrediente)
        REFERENCES ingredientes(IdIngrediente)
        ON DELETE CASCADE
        ON UPDATE CASCADE
);


-- =====================================================
-- TABLA: PEDIDOS
-- =====================================================

CREATE TABLE pedidos (
    IdPedido INT AUTO_INCREMENT PRIMARY KEY,
    IdMesero INT NOT NULL,
    IdMesa INT NULL,
    TipoPedido VARCHAR(30) NOT NULL,
    Fecha DATE NOT NULL,
    HoraInicio TIME NOT NULL,
    HoraFin TIME NULL,
    Estado VARCHAR(30) NOT NULL DEFAULT 'Pendiente',

    CONSTRAINT fk_pedido_mesero
        FOREIGN KEY (IdMesero)
        REFERENCES usuarios(IdUsuario)
        ON DELETE RESTRICT
        ON UPDATE CASCADE,

    CONSTRAINT fk_pedido_mesa
        FOREIGN KEY (IdMesa)
        REFERENCES mesas(IdMesa)
        ON DELETE SET NULL
        ON UPDATE CASCADE
);


-- =====================================================
-- TABLA: DETALLE_PEDIDO
-- =====================================================

CREATE TABLE detalle_pedido (
    IdDetalle INT AUTO_INCREMENT PRIMARY KEY,
    IdPedido INT NOT NULL,
    IdPlato INT NOT NULL,
    Cantidad INT NOT NULL,
    IndicacionesExtra VARCHAR(255),

    CONSTRAINT fk_detalle_pedido
        FOREIGN KEY (IdPedido)
        REFERENCES pedidos(IdPedido)
        ON DELETE CASCADE
        ON UPDATE CASCADE,

    CONSTRAINT fk_detalle_plato
        FOREIGN KEY (IdPlato)
        REFERENCES platos(IdPlato)
        ON DELETE RESTRICT
        ON UPDATE CASCADE
);


-- =====================================================
-- TABLA: COMPRAS
-- =====================================================

CREATE TABLE compras (
    IdCompra INT AUTO_INCREMENT PRIMARY KEY,
    IdUsuario INT NOT NULL,
    Fecha DATE NOT NULL,
    Proveedor VARCHAR(150) NOT NULL,

    CONSTRAINT fk_compra_usuario
        FOREIGN KEY (IdUsuario)
        REFERENCES usuarios(IdUsuario)
        ON DELETE RESTRICT
        ON UPDATE CASCADE
);


-- =====================================================
-- TABLA: DETALLE_COMPRA
-- =====================================================

CREATE TABLE detalle_compra (
    IdDetalleCompra INT AUTO_INCREMENT PRIMARY KEY,
    IdCompra INT NOT NULL,
    IdIngrediente INT NOT NULL,
    Cantidad DECIMAL(10,2) NOT NULL,

    CONSTRAINT fk_detalle_compra
        FOREIGN KEY (IdCompra)
        REFERENCES compras(IdCompra)
        ON DELETE CASCADE
        ON UPDATE CASCADE,

    CONSTRAINT fk_detalle_compra_ingrediente
        FOREIGN KEY (IdIngrediente)
        REFERENCES ingredientes(IdIngrediente)
        ON DELETE RESTRICT
        ON UPDATE CASCADE
);


-- =====================================================
-- TABLA: INVENTARIO
-- =====================================================

CREATE TABLE inventario (
    IdInventario INT AUTO_INCREMENT PRIMARY KEY,
    IdIngrediente INT NOT NULL,
    TipoMovimiento VARCHAR(30) NOT NULL,
    Cantidad DECIMAL(10,2) NOT NULL,
    Fecha DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Motivo VARCHAR(255),

    CONSTRAINT fk_inventario_ingrediente
        FOREIGN KEY (IdIngrediente)
        REFERENCES ingredientes(IdIngrediente)
        ON DELETE RESTRICT
        ON UPDATE CASCADE
);