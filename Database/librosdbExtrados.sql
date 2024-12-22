CREATE TABLE Users (
    ID INT AUTO_INCREMENT PRIMARY KEY,
    Nombre VARCHAR(100) NOT NULL,
    Edad TINYINT NOT NULL CHECK (Edad >= 14),
    Email VARCHAR(100) NOT NULL UNIQUE,
    DNI INT NOT NULL UNIQUE,
    Deleted BOOLEAN DEFAULT FALSE,
    Rol ENUM('Admin','User') DEFAULT 'User',
    Password VARCHAR(255) NOT NULL
);

CREATE TABLE Books (
    ID INT AUTO_INCREMENT PRIMARY KEY,
    Titulo VARCHAR(100) NOT NULL,
    Autor VARCHAR(100),
    Descripcion VARCHAR(255),
    FechaPublicacion DATE,
    UserID INT,
    FOREIGN KEY (UserID) REFERENCES Users(ID) ON DELETE CASCADE
);
CREATE TABLE RentBooks (
    UserID INT,
    BookID INT,
    FechaPrestamo DATETIME NOT NULL,
    FechaVencimiento DATETIME NOT NULL,
    Status ENUM('Activo', 'Devuelto','Disponible') DEFAULT 'Disponible',
    PRIMARY KEY (UserID, BookID),
    FOREIGN KEY (UserID) REFERENCES Users(ID) ON DELETE CASCADE,
    FOREIGN KEY (BookID) REFERENCES Books(ID) ON DELETE CASCADE
);

CREATE USER 'user'@'localhost' IDENTIFIED BY 'clientpassword';
CREATE USER 'admin'@'localhost' IDENTIFIED BY 'adminpassword';

-- admin privileges
GRANT ALL PRIVILEGES ON extradosdb.* TO 'admin'@'localhost';

-- user privileges (see books / rent books / modify personal information)
GRANT SELECT, INSERT, UPDATE, DELETE ON extradosdb.Books TO 'user'@'localhost';
GRANT SELECT, INSERT, UPDATE ON extradosdb.RentBooks TO 'user'@'localhost';
GRANT SELECT, INSERT, UPDATE ON extradosdb.Users TO 'user'@'localhost';

FLUSH PRIVILEGES;
