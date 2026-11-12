using EcosistemaAcademico.Models.Entidades;

namespace EcosistemaAcademico.Models.Data
{
    public static class DatosDemo
    {
        public static void Sembrar(AppDbContext db)
        {
            // Si ya se cargaron, no se repite
            if (db.Usuarios.Any(u => u.Correo == "carlos.mendez@demo.com")) return;

            var admin = db.Administradores.First();
            var ahora = DateTime.Now;
            var hashEst = BCrypt.Net.BCrypt.HashPassword("Est123*");
            var hashTut = BCrypt.Net.BCrypt.HashPassword("Tutor123*");

            // ---- Funciones auxiliares para no repetir código ----
            Asignatura Asig(string codigo, string nombre, int creditos, string area, int nivel) => new()
            {
                Codigo = codigo,
                Nombre = nombre,
                Creditos = creditos,
                Area = area,
                Nivel = nivel,
                Descripcion = "Curso de " + nombre,
                Estado = true
            };

            Estudiante Est(string nombre, string apellido, string correo, string matricula,
                           string carrera, int nivel, decimal promedio, bool intervencion) => new()
                           {
                               Nombre = nombre,
                               Apellido = apellido,
                               Correo = correo,
                               PasswordHash = hashEst,
                               Rol = "Estudiante",
                               FechaRegistro = ahora,
                               Estado = true,
                               Matricula = matricula,
                               Carrera = carrera,
                               Nivel = nivel,
                               Promedio = promedio,
                               NecesitaIntervencion = intervencion
                           };

            Tutor Tut(string nombre, string apellido, string correo, string especialidad,
                      string disponibilidad, decimal calificacion, int capacidad) => new()
                      {
                          Nombre = nombre,
                          Apellido = apellido,
                          Correo = correo,
                          PasswordHash = hashTut,
                          Rol = "Tutor",
                          FechaRegistro = ahora,
                          Estado = true,
                          Especialidad = especialidad,
                          Disponibilidad = disponibilidad,
                          CalificacionPromedio = calificacion,
                          CapacidadMaxima = capacidad
                      };

            TutorAsignatura TA(Tutor t, Asignatura a, string nivel, int anios) => new()
            {
                Tutor = t,
                Asignatura = a,
                NivelDominio = nivel,
                AniosExperiencia = anios,
                FechaAsignacion = ahora,
                Estado = true
            };

            DiagnosticoAcademico Diag(Estudiante e, Asignatura a, string riesgo,
                                      string fortalezas, string debilidades, string obs) => new()
                                      {
                                          Estudiante = e,
                                          Asignatura = a,
                                          NivelRiesgo = riesgo,
                                          Fortalezas = fortalezas,
                                          Debilidades = debilidades,
                                          Fecha = ahora.AddDays(-14),
                                          Observaciones = obs
                                      };

            RecursoAcademico Rec(string titulo, string tipo, Asignatura a, Repositorio r, Tutor t, string url) => new()
            {
                Titulo = titulo,
                Tipo = tipo,
                Asignatura = a,
                Repositorio = r,
                Tutor = t,
                UrlRuta = url,
                FechaSubida = ahora.AddDays(-7),
                Estado = true
            };

            // ---- Asignaturas ----
            var mat = Asig("MAT-101", "Matemática I", 4, "Matemática", 1);
            var len = Asig("LEN-101", "Lengua Española", 3, "Lenguas", 1);
            var fis = Asig("FIS-101", "Física I", 4, "Ciencias", 2);
            var qui = Asig("QUI-101", "Química General", 4, "Ciencias", 2);
            var prg = Asig("PRG-101", "Programación I", 4, "Informática", 1);
            var ing = Asig("ING-101", "Inglés Básico", 3, "Lenguas", 1);

            // ---- Estudiantes ----
            var carlos = Est("Carlos", "Méndez", "carlos.mendez@demo.com", "2026-0001", "Informática", 2, 62.5m, true);
            var ana = Est("Ana", "Rodríguez", "ana.rodriguez@demo.com", "2026-0002", "Educación", 1, 74m, false);
            var luis = Est("Luis", "Peña", "luis.pena@demo.com", "2026-0003", "Ingeniería Civil", 3, 70m, false);
            var maria = Est("María", "Santos", "maria.santos@demo.com", "2026-0004", "Ingeniería Civil", 2, 55m, true);
            var jose = Est("José", "Almonte", "jose.almonte@demo.com", "2026-0005", "Informática", 1, 58.5m, true);
            var laura = Est("Laura", "Féliz", "laura.feliz@demo.com", "2026-0006", "Administración", 1, 88m, false);

            // ---- Tutores ----
            var rafael = Tut("Rafael", "Pérez", "rafael.perez@demo.com", "Matemática", "Lunes y miércoles 2-4 pm", 4.6m, 3);
            var carmen = Tut("Carmen", "Vásquez", "carmen.vasquez@demo.com", "Lengua y literatura", "Martes y jueves 3-5 pm", 4.2m, 4);
            var pedro = Tut("Pedro", "Martínez", "pedro.martinez@demo.com", "Ciencias", "Lunes a viernes 4-6 pm", 4.8m, 3);
            var sofia = Tut("Sofía", "Reyes", "sofia.reyes@demo.com", "Informática e inglés", "Sábados 9 am-1 pm", 4.5m, 4);

            // ---- Repositorios ----
            var central = new Repositorio
            {
                Nombre = "Repositorio Central",
                Sede = "Barahona",
                Ubicacion = "Biblioteca principal",
                Tipo = "Local",
                CapacidadGB = 500,
                Estado = true,
                Administrador = admin
            };
            var nube = new Repositorio
            {
                Nombre = "Repositorio en la nube",
                Sede = "Santo Domingo",
                Ubicacion = "Servidor externo",
                Tipo = "Nube",
                CapacidadGB = 1000,
                Estado = true,
                Administrador = admin
            };

            // ---- Emparejamientos (puntajes calculados con la fórmula del Paso 15) ----
            var e1 = new Emparejamiento { Estudiante = carlos, Tutor = rafael, Asignatura = mat, PuntajeAfinidad = 89.6, Fecha = ahora.AddDays(-10), Estado = "Activo" };
            var e2 = new Emparejamiento { Estudiante = ana, Tutor = carmen, Asignatura = len, PuntajeAfinidad = 91.2, Fecha = ahora.AddDays(-2), Estado = "Pendiente" };
            var e3 = new Emparejamiento { Estudiante = luis, Tutor = pedro, Asignatura = fis, PuntajeAfinidad = 76.8, Fecha = ahora.AddDays(-20), Estado = "Finalizado" };

            // ---- Guardado ----
            db.Asignaturas.AddRange(mat, len, fis, qui, prg, ing);
            db.Estudiantes.AddRange(carlos, ana, luis, maria, jose, laura);
            db.Tutores.AddRange(rafael, carmen, pedro, sofia);
            db.Repositorios.AddRange(central, nube);

            db.TutorAsignaturas.AddRange(
                TA(rafael, mat, "Avanzado", 6), TA(rafael, fis, "Básico", 1),
                TA(carmen, len, "Avanzado", 8), TA(carmen, qui, "Básico", 1),
                TA(pedro, fis, "Intermedio", 4), TA(pedro, qui, "Avanzado", 5), TA(pedro, prg, "Intermedio", 2),
                TA(sofia, prg, "Avanzado", 5), TA(sofia, ing, "Intermedio", 3), TA(sofia, mat, "Básico", 2));

            db.Diagnosticos.AddRange(
                Diag(carlos, mat, "Alto", "Buena actitud", "Álgebra y fracciones", "Necesita refuerzo urgente"),
                Diag(ana, len, "Medio", "Lectura comprensiva", "Ortografía", "Practicar redacción"),
                Diag(luis, fis, "Medio", "Razonamiento lógico", "Cinemática", "Va mejorando"),
                Diag(maria, qui, "Alto", "Participa en clase", "Balanceo de ecuaciones", "Riesgo de reprobar"),
                Diag(jose, prg, "Alto", "Creatividad", "Lógica de programación", "Sin base previa"),
                Diag(laura, ing, "Bajo", "Vocabulario amplio", "Ninguna importante", "Desempeño sobresaliente"));

            db.Emparejamientos.AddRange(e1, e2, e3);

            db.Recursos.AddRange(
                Rec("Guía de álgebra básica", "PDF", mat, central, rafael, "https://example.com/algebra.pdf"),
                Rec("Ortografía y redacción", "Guía", len, central, carmen, "https://example.com/ortografia"),
                Rec("Leyes de Newton", "Video", fis, nube, pedro, "https://example.com/newton"),
                Rec("Tabla periódica interactiva", "Enlace", qui, nube, pedro, "https://example.com/tabla-periodica"),
                Rec("Introducción a C#", "Presentación", prg, central, sofia, "https://example.com/csharp-intro"));

            db.Seguimientos.AddRange(
                new SeguimientoAcademico
                {
                    Emparejamiento = e1,
                    Fecha = ahora.AddDays(-7),
                    Progreso = 30,
                    CalificacionAntes = 55m,
                    CalificacionDespues = 62m,
                    Observaciones = "Repasó fracciones y ecuaciones simples",
                    ProximaSesion = ahora.AddDays(-3)
                },
                new SeguimientoAcademico
                {
                    Emparejamiento = e1,
                    Fecha = ahora.AddDays(-3),
                    Progreso = 60,
                    CalificacionAntes = 62m,
                    CalificacionDespues = 70m,
                    Observaciones = "Dominó ecuaciones de primer grado",
                    ProximaSesion = ahora.AddDays(4)
                },
                new SeguimientoAcademico
                {
                    Emparejamiento = e3,
                    Fecha = ahora.AddDays(-2),
                    Progreso = 100,
                    CalificacionAntes = 60m,
                    CalificacionDespues = 85m,
                    Observaciones = "Completó el plan de tutoría",
                    ProximaSesion = null
                });

            db.SaveChanges();
        }
    }
}
