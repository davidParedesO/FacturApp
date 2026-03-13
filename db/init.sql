-- Script de inicialización de la base de datos facturacion_db
-- Se ejecuta automáticamente al levantar el contenedor Docker por primera vez

CREATE TABLE IF NOT EXISTS clientes (
  id SERIAL PRIMARY KEY,
  nombre VARCHAR(200) NOT NULL,
  email VARCHAR(200),
  direccion TEXT,
  nif VARCHAR(20),
  telefono VARCHAR(20),
  created_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS productos (
  id SERIAL PRIMARY KEY,
  nombre VARCHAR(200) NOT NULL,
  descripcion TEXT,
  precio DECIMAL(10,2) NOT NULL,
  stock INTEGER NOT NULL DEFAULT 0,
  created_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS facturas (
  id SERIAL PRIMARY KEY,
  numero_factura VARCHAR(50) UNIQUE NOT NULL,
  cliente_id INTEGER REFERENCES clientes(id),
  fecha_emision TIMESTAMPTZ DEFAULT NOW(),
  subtotal DECIMAL(10,2),
  iva DECIMAL(10,2),
  total DECIMAL(10,2),
  estado VARCHAR(50) DEFAULT 'PENDIENTE',
  pdf_base64 TEXT,
  origen VARCHAR(50) DEFAULT 'VOZ'
);

CREATE TABLE IF NOT EXISTS lineas_factura (
  id SERIAL PRIMARY KEY,
  factura_id INTEGER REFERENCES facturas(id) ON DELETE CASCADE,
  producto_id INTEGER REFERENCES productos(id),
  cantidad INTEGER NOT NULL,
  precio_unitario DECIMAL(10,2) NOT NULL,
  subtotal DECIMAL(10,2) NOT NULL
);

-- Datos de ejemplo
INSERT INTO clientes (nombre, email, nif, direccion, telefono) VALUES
  ('Juan Pérez', 'juan@ejemplo.com', '12345678A', 'Calle Mayor 1, Madrid', '600000001'),
  ('María García', 'maria@ejemplo.com', '87654321B', 'Av. Libertad 5, Barcelona', '600000002'),
  ('Carlos López', 'carlos@empresa.com', '11223344C', 'Plaza España 10, Sevilla', '600000003')
ON CONFLICT DO NOTHING;

INSERT INTO productos (nombre, descripcion, precio, stock) VALUES
  ('Monitor', 'Monitor Full HD 27"', 299.99, 50),
  ('Teclado', 'Teclado mecánico RGB', 89.99, 100),
  ('Ordenador', 'PC de sobremesa i7 16GB RAM', 899.99, 20),
  ('Gráfica', 'Tarjeta gráfica RTX 4060 8GB', 499.99, 15),
  ('Ratón', 'Ratón inalámbrico ergonómico', 49.99, 200),
  ('Auriculares', 'Auriculares gaming con micrófono', 79.99, 75)
ON CONFLICT DO NOTHING;
