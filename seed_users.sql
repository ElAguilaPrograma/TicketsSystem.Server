-- Seed script: 100 Users (10 Admins, 30 Agents, 60 Users)
-- Table: "Users"
-- Columns: "UserId" (uuid), "FullName", "Email", "PasswordHash", "Role", "IsActive", "ProfilePicUrl", "ProfilePicPath", "CreatedAt"
-- Default password hash corresponds to BCrypt of "Password123!"
-- ProfilePicUrl and ProfilePicPath left as NULL

BEGIN;

-- 10 Admins
INSERT INTO "Users" ("UserId", "FullName", "Email", "PasswordHash", "Role", "IsActive", "ProfilePicUrl", "ProfilePicPath", "CreatedAt")
VALUES
  ('a1b2c3d4-0001-4000-8000-000000000001', 'Carlos Mendoza',       'carlos.mendoza@ticketsystem.com',       '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'Admin', true, NULL, NULL, '2025-01-05T09:00:00Z'),
  ('a1b2c3d4-0001-4000-8000-000000000002', 'María Fernández',      'maria.fernandez@ticketsystem.com',      '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'Admin', true, NULL, NULL, '2025-01-07T10:30:00Z'),
  ('a1b2c3d4-0001-4000-8000-000000000003', 'Roberto García',        'roberto.garcia@ticketsystem.com',       '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'Admin', true, NULL, NULL, '2025-01-10T08:15:00Z'),
  ('a1b2c3d4-0001-4000-8000-000000000004', 'Ana Martínez',          'ana.martinez@ticketsystem.com',         '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'Admin', true, NULL, NULL, '2025-01-12T14:00:00Z'),
  ('a1b2c3d4-0001-4000-8000-000000000005', 'Diego López',           'diego.lopez@ticketsystem.com',          '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'Admin', true, NULL, NULL, '2025-01-15T11:45:00Z'),
  ('a1b2c3d4-0001-4000-8000-000000000006', 'Laura Sánchez',         'laura.sanchez@ticketsystem.com',        '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'Admin', true, NULL, NULL, '2025-01-18T09:30:00Z'),
  ('a1b2c3d4-0001-4000-8000-000000000007', 'Fernando Torres',       'fernando.torres@ticketsystem.com',      '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'Admin', true, NULL, NULL, '2025-01-20T16:20:00Z'),
  ('a1b2c3d4-0001-4000-8000-000000000008', 'Patricia Ruiz',         'patricia.ruiz@ticketsystem.com',        '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'Admin', true, NULL, NULL, '2025-01-22T13:10:00Z'),
  ('a1b2c3d4-0001-4000-8000-000000000009', 'Javier Morales',        'javier.morales@ticketsystem.com',       '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'Admin', true, NULL, NULL, '2025-01-25T10:00:00Z'),
  ('a1b2c3d4-0001-4000-8000-000000000010', 'Lucía Castillo',        'lucia.castillo@ticketsystem.com',       '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'Admin', true, NULL, NULL, '2025-01-28T08:50:00Z');

-- 30 Agents
INSERT INTO "Users" ("UserId", "FullName", "Email", "PasswordHash", "Role", "IsActive", "ProfilePicUrl", "ProfilePicPath", "CreatedAt")
VALUES
  ('b1b2c3d4-0002-4000-8000-000000000001', 'Andrés Jiménez',        'andres.jimenez@ticketsystem.com',       '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'Agent', true, NULL, NULL, '2025-02-01T09:00:00Z'),
  ('b1b2c3d4-0002-4000-8000-000000000002', 'Sofía Navarro',         'sofia.navarro@ticketsystem.com',        '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'Agent', true, NULL, NULL, '2025-02-01T09:10:00Z'),
  ('b1b2c3d4-0002-4000-8000-000000000003', 'Miguel Herrera',        'miguel.herrera@ticketsystem.com',       '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'Agent', true, NULL, NULL, '2025-02-01T09:20:00Z'),
  ('b1b2c3d4-0002-4000-8000-000000000004', 'Elena Romero',          'elena.romero@ticketsystem.com',         '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'Agent', true, NULL, NULL, '2025-02-02T10:00:00Z'),
  ('b1b2c3d4-0002-4000-8000-000000000005', 'Raúl Ortega',           'raul.ortega@ticketsystem.com',          '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'Agent', true, NULL, NULL, '2025-02-02T10:15:00Z'),
  ('b1b2c3d4-0002-4000-8000-000000000006', 'Isabel Delgado',        'isabel.delgado@ticketsystem.com',       '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'Agent', true, NULL, NULL, '2025-02-02T10:30:00Z'),
  ('b1b2c3d4-0002-4000-8000-000000000007', 'Pablo Serrano',         'pablo.serrano@ticketsystem.com',        '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'Agent', true, NULL, NULL, '2025-02-03T08:45:00Z'),
  ('b1b2c3d4-0002-4000-8000-000000000008', 'Carmen Vega',           'carmen.vega@ticketsystem.com',          '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'Agent', true, NULL, NULL, '2025-02-03T09:00:00Z'),
  ('b1b2c3d4-0002-4000-8000-000000000009', 'Alejandro Muñoz',       'alejandro.munoz@ticketsystem.com',      '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'Agent', true, NULL, NULL, '2025-02-03T09:20:00Z'),
  ('b1b2c3d4-0002-4000-8000-000000000010', 'Natalia Reyes',         'natalia.reyes@ticketsystem.com',        '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'Agent', true, NULL, NULL, '2025-02-04T11:00:00Z'),
  ('b1b2c3d4-0002-4000-8000-000000000011', 'Tomás Medina',          'tomas.medina@ticketsystem.com',         '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'Agent', true, NULL, NULL, '2025-02-04T11:30:00Z'),
  ('b1b2c3d4-0002-4000-8000-000000000012', 'Valeria Cruz',          'valeria.cruz@ticketsystem.com',         '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'Agent', true, NULL, NULL, '2025-02-05T08:30:00Z'),
  ('b1b2c3d4-0002-4000-8000-000000000013', 'Ricardo Flores',        'ricardo.flores@ticketsystem.com',       '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'Agent', true, NULL, NULL, '2025-02-05T09:00:00Z'),
  ('b1b2c3d4-0002-4000-8000-000000000014', 'Adriana Peña',          'adriana.pena@ticketsystem.com',         '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'Agent', true, NULL, NULL, '2025-02-06T10:00:00Z'),
  ('b1b2c3d4-0002-4000-8000-000000000015', 'Eduardo Bravo',         'eduardo.bravo@ticketsystem.com',        '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'Agent', true, NULL, NULL, '2025-02-06T10:45:00Z'),
  ('b1b2c3d4-0002-4000-8000-000000000016', 'Gabriela Campos',       'gabriela.campos@ticketsystem.com',      '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'Agent', true, NULL, NULL, '2025-02-07T08:00:00Z'),
  ('b1b2c3d4-0002-4000-8000-000000000017', 'Martín Acosta',         'martin.acosta@ticketsystem.com',        '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'Agent', true, NULL, NULL, '2025-02-07T09:15:00Z'),
  ('b1b2c3d4-0002-4000-8000-000000000018', 'Clara Vargas',          'clara.vargas@ticketsystem.com',         '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'Agent', true, NULL, NULL, '2025-02-08T11:30:00Z'),
  ('b1b2c3d4-0002-4000-8000-000000000019', 'Héctor Paredes',       'hector.paredes@ticketsystem.com',       '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'Agent', true, NULL, NULL, '2025-02-08T12:00:00Z'),
  ('b1b2c3d4-0002-4000-8000-000000000020', 'Rosa Gallardo',         'rosa.gallardo@ticketsystem.com',        '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'Agent', true, NULL, NULL, '2025-02-10T08:00:00Z'),
  ('b1b2c3d4-0002-4000-8000-000000000021', 'Daniel Ríos',          'daniel.rios@ticketsystem.com',          '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'Agent', true, NULL, NULL, '2025-02-10T08:30:00Z'),
  ('b1b2c3d4-0002-4000-8000-000000000022', 'Alicia Mora',           'alicia.mora@ticketsystem.com',          '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'Agent', true, NULL, NULL, '2025-02-11T09:00:00Z'),
  ('b1b2c3d4-0002-4000-8000-000000000023', 'Óscar León',            'oscar.leon@ticketsystem.com',           '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'Agent', true, NULL, NULL, '2025-02-11T09:30:00Z'),
  ('b1b2c3d4-0002-4000-8000-000000000024', 'Beatriz Gil',           'beatriz.gil@ticketsystem.com',          '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'Agent', true, NULL, NULL, '2025-02-12T10:00:00Z'),
  ('b1b2c3d4-0002-4000-8000-000000000025', 'Félix Cano',            'felix.cano@ticketsystem.com',           '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'Agent', true, NULL, NULL, '2025-02-12T10:45:00Z'),
  ('b1b2c3d4-0002-4000-8000-000000000026', 'Teresa Ibáñez',        'teresa.ibanez@ticketsystem.com',        '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'Agent', true, NULL, NULL, '2025-02-13T08:15:00Z'),
  ('b1b2c3d4-0002-4000-8000-000000000027', 'Mario Castro',          'mario.castro@ticketsystem.com',         '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'Agent', true, NULL, NULL, '2025-02-13T08:45:00Z'),
  ('b1b2c3d4-0002-4000-8000-000000000028', 'Inés Reguera',          'ines.reguera@ticketsystem.com',         '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'Agent', true, NULL, NULL, '2025-02-14T11:00:00Z'),
  ('b1b2c3d4-0002-4000-8000-000000000029', 'Marco Sanz',            'marco.sanz@ticketsystem.com',           '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'Agent', true, NULL, NULL, '2025-02-14T11:30:00Z'),
  ('b1b2c3d4-0002-4000-8000-000000000030', 'Silvia Hidalgo',        'silvia.hidalgo@ticketsystem.com',       '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'Agent', true, NULL, NULL, '2025-02-15T09:00:00Z');

-- 60 Users (end users / clientes)
INSERT INTO "Users" ("UserId", "FullName", "Email", "PasswordHash", "Role", "IsActive", "ProfilePicUrl", "ProfilePicPath", "CreatedAt")
VALUES
  ('c1b2c3d4-0003-4000-8000-000000000001', 'Juan Pérez',            'juan.perez@ticketsystem.com',           '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-02-20T10:00:00Z'),
  ('c1b2c3d4-0003-4000-8000-000000000002', 'María González',        'maria.gonzalez@ticketsystem.com',       '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-02-20T10:15:00Z'),
  ('c1b2c3d4-0003-4000-8000-000000000003', 'Pedro Rodríguez',       'pedro.rodriguez@ticketsystem.com',      '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-02-21T09:00:00Z'),
  ('c1b2c3d4-0003-4000-8000-000000000004', 'Lucía Hernández',       'lucia.hernandez@ticketsystem.com',      '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-02-21T09:30:00Z'),
  ('c1b2c3d4-0003-4000-8000-000000000005', 'Antonio Díaz',          'antonio.diaz@ticketsystem.com',         '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-02-22T08:00:00Z'),
  ('c1b2c3d4-0003-4000-8000-000000000006', 'Carmen López',          'carmen.lopez@ticketsystem.com',         '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-02-22T08:45:00Z'),
  ('c1b2c3d4-0003-4000-8000-000000000007', 'José Moreno',           'jose.moreno@ticketsystem.com',          '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-02-23T10:30:00Z'),
  ('c1b2c3d4-0003-4000-8000-000000000008', 'Isabel Muñoz',          'isabel.munoz@ticketsystem.com',         '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-02-23T11:00:00Z'),
  ('c1b2c3d4-0003-4000-8000-000000000009', 'Francisco Álvarez',     'francisco.alvarez@ticketsystem.com',    '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-02-24T09:00:00Z'),
  ('c1b2c3d4-0003-4000-8000-000000000010', 'Elena Romero',          'elena.r@ticketsystem.com',              '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-02-24T09:30:00Z'),
  ('c1b2c3d4-0003-4000-8000-000000000011', 'David Torres',          'david.torres@ticketsystem.com',         '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-02-25T08:00:00Z'),
  ('c1b2c3d4-0003-4000-8000-000000000012', 'Pilar Sánchez',         'pilar.sanchez@ticketsystem.com',        '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-02-25T08:30:00Z'),
  ('c1b2c3d4-0003-4000-8000-000000000013', 'Sergio Martín',         'sergio.martin@ticketsystem.com',        '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-02-26T10:00:00Z'),
  ('c1b2c3d4-0003-4000-8000-000000000014', 'Rosa Jiménez',          'rosa.jimenez@ticketsystem.com',         '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-02-26T10:30:00Z'),
  ('c1b2c3d4-0003-4000-8000-000000000015', 'Jorge Navarro',         'jorge.navarro@ticketsystem.com',        '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-02-27T09:00:00Z'),
  ('c1b2c3d4-0003-4000-8000-000000000016', 'Teresa Díez',           'teresa.diez@ticketsystem.com',          '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-02-27T09:45:00Z'),
  ('c1b2c3d4-0003-4000-8000-000000000017', 'Andrés Serrano',        'andres.serrano@ticketsystem.com',       '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-02-28T11:00:00Z'),
  ('c1b2c3d4-0003-4000-8000-000000000018', 'Cristina Molina',       'cristina.molina@ticketsystem.com',      '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-02-28T11:30:00Z'),
  ('c1b2c3d4-0003-4000-8000-000000000019', 'Manuel Ortega',         'manuel.ortega@ticketsystem.com',        '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-03-01T08:00:00Z'),
  ('c1b2c3d4-0003-4000-8000-000000000020', 'Amparo Ruiz',          'amparo.ruiz@ticketsystem.com',          '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-03-01T08:30:00Z'),
  ('c1b2c3d4-0003-4000-8000-000000000021', 'Rafael Delgado',        'rafael.delgado@ticketsystem.com',       '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-03-02T10:00:00Z'),
  ('c1b2c3d4-0003-4000-8000-000000000022', 'Sandra Castro',         'sandra.castro@ticketsystem.com',       '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-03-02T10:20:00Z'),
  ('c1b2c3d4-0003-4000-8000-000000000023', 'Emilio Ferrer',         'emilio.ferrer@ticketsystem.com',        '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-03-03T09:00:00Z'),
  ('c1b2c3d4-0003-4000-8000-000000000024', 'Marta Lorenzo',        'marta.lorenzo@ticketsystem.com',        '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-03-03T09:30:00Z'),
  ('c1b2c3d4-0003-4000-8000-000000000025', 'Iván Prieto',          'ivan.prieto@ticketsystem.com',          '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-03-04T08:15:00Z'),
  ('c1b2c3d4-0003-4000-8000-000000000026', 'Olivia Peña',          'olivia.pena@ticketsystem.com',          '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-03-04T08:45:00Z'),
  ('c1b2c3d4-0003-4000-8000-000000000027', 'Fernando Gallego',      'fernando.gallego@ticketsystem.com',     '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-03-05T10:30:00Z'),
  ('c1b2c3d4-0003-4000-8000-000000000028', 'Laura Vidal',           'laura.vidal@ticketsystem.com',          '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-03-05T11:00:00Z'),
  ('c1b2c3d4-0003-4000-8000-000000000029', 'Alberto Cantó',         'alberto.canto@ticketsystem.com',        '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-03-06T09:00:00Z'),
  ('c1b2c3d4-0003-4000-8000-000000000030', 'Nuria Esteban',        'nuria.esteban@ticketsystem.com',        '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-03-06T09:30:00Z'),
  ('c1b2c3d4-0003-4000-8000-000000000031', 'Guillermo Marín',      'guillermo.marin@ticketsystem.com',      '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-03-07T08:00:00Z'),
  ('c1b2c3d4-0003-4000-8000-000000000032', 'Pilar Crespo',         'pilar.crespo@ticketsystem.com',         '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-03-07T08:30:00Z'),
  ('c1b2c3d4-0003-4000-8000-000000000033', 'Vicente Ibáñez',       'vicente.ibanez@ticketsystem.com',       '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-03-08T10:00:00Z'),
  ('c1b2c3d4-0003-4000-8000-000000000034', 'Carolina León',        'carolina.leon@ticketsystem.com',        '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-03-08T10:30:00Z'),
  ('c1b2c3d4-0003-4000-8000-000000000035', 'Óscar Garrido',        'oscar.garrido@ticketsystem.com',        '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-03-09T09:00:00Z'),
  ('c1b2c3d4-0003-4000-8000-000000000036', 'Alicia Pons',          'alicia.pons@ticketsystem.com',          '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-03-09T09:30:00Z'),
  ('c1b2c3d4-0003-4000-8000-000000000037', 'Enrique Mata',         'enrique.mata@ticketsystem.com',         '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-03-10T08:15:00Z'),
  ('c1b2c3d4-0003-4000-8000-000000000038', 'Beatriz Fuentes',      'beatriz.fuentes@ticketsystem.com',      '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-03-10T08:45:00Z'),
  ('c1b2c3d4-0003-4000-8000-000000000039', 'Alfonso Cadenas',      'alfonso.cadenas@ticketsystem.com',      '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-03-11T11:00:00Z'),
  ('c1b2c3d4-0003-4000-8000-000000000040', 'Rosa Iglesias',        'rosa.iglesias@ticketsystem.com',        '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-03-11T11:30:00Z'),
  ('c1b2c3d4-0003-4000-8000-000000000041', 'Diego Moya',           'diego.moya@ticketsystem.com',           '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-03-12T09:00:00Z'),
  ('c1b2c3d4-0003-4000-8000-000000000042', 'Mónica Merino',        'monica.merino@ticketsystem.com',        '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-03-12T09:30:00Z'),
  ('c1b2c3d4-0003-4000-8000-000000000043', 'Roberto Cortés',       'roberto.cortes@ticketsystem.com',       '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-03-13T08:00:00Z'),
  ('c1b2c3d4-0003-4000-8000-000000000044', 'Aitana Blasco',        'aitana.blasco@ticketsystem.com',         '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-03-13T08:30:00Z'),
  ('c1b2c3d4-0003-4000-8000-000000000045', 'Hugo Mármol',          'hugo.marmol@ticketsystem.com',          '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-03-14T10:00:00Z'),
  ('c1b2c3d4-0003-4000-8000-000000000046', 'Ana Belén Escudero',   'anab.escudero@ticketsystem.com',       '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-03-14T10:30:00Z'),
  ('c1b2c3d4-0003-4000-8000-000000000047', 'Tomás Cebrián',        'tomas.cebrian@ticketsystem.com',        '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-03-15T09:00:00Z'),
  ('c1b2c3d4-0003-4000-8000-000000000048', 'Clara Espinosa',       'clara.espinosa@ticketsystem.com',       '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-03-15T09:30:00Z'),
  ('c1b2c3d4-0003-4000-8000-000000000049', 'Marcos Vela',          'marcos.vela@ticketsystem.com',          '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-03-16T08:00:00Z'),
  ('c1b2c3d4-0003-4000-8000-000000000050', 'Lorena Canales',        'lorena.canales@ticketsystem.com',       '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-03-16T08:30:00Z'),
  ('c1b2c3d4-0003-4000-8000-000000000051', 'Rubén Tarrés',         'ruben.tarres@ticketsystem.com',         '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-03-17T11:00:00Z'),
  ('c1b2c3d4-0003-4000-8000-000000000052', 'Salomé Rohan',         'salome.rohan@ticketsystem.com',         '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-03-17T11:30:00Z'),
  ('c1b2c3d4-0003-4000-8000-000000000053', 'Julián Carrera',       'julian.carrera@ticketsystem.com',       '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-03-18T09:00:00Z'),
  ('c1b2c3d4-0003-4000-8000-000000000054', 'Ariadna Pacheco',      'ariadna.pacheco@ticketsystem.com',      '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-03-18T09:30:00Z'),
  ('c1b2c3d4-0003-4000-8000-000000000055', 'Emilia Soler',         'emilia.soler@ticketsystem.com',         '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-03-19T08:00:00Z'),
  ('c1b2c3d4-0003-4000-8000-000000000056', 'Ángel Reguera',        'angel.reguera@ticketsystem.com',        '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-03-19T08:30:00Z'),
  ('c1b2c3d4-0003-4000-8000-000000000057', 'Verónica Llorente',     'veronica.llorente@ticketsystem.com',    '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-03-20T10:00:00Z'),
  ('c1b2c3d4-0003-4000-8000-000000000058', 'César Bueno',          'cesar.bueno@ticketsystem.com',          '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-03-20T10:15:00Z'),
  ('c1b2c3d4-0003-4000-8000-000000000059', 'Begoña Simón',         'begona.simon@ticketsystem.com',         '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-03-21T09:00:00Z'),
  ('c1b2c3d4-0003-4000-8000-000000000060', 'Ignacio Belmonte',     'ignacio.belmonte@ticketsystem.com',    '$2a$11$V8LKcGyE3vVPQh7mGQJxkeYFqKOqqDXW3p5vZQKrF3EZzDnP6HKaC', 'User', true, NULL, NULL, '2025-03-21T09:30:00Z');

COMMIT;