# Autenticación - Recuperación de contraseña

## Objetivo
Documentar la secuencia actual para cuando un usuario olvida su contraseña en `Zenith`.

## Estado actual del flujo
- La recuperación automática por correo no está habilitada.
- La pantalla `¿Olvidaste tu contraseña?` informa al usuario que debe solicitar apoyo a un administrador.
- El administrador puede generar una contraseña temporal desde `Administración > Usuarios`.
- El usuario inicia sesión con esa contraseña temporal.
- El sistema obliga al usuario a capturar una nueva contraseña al entrar.

## Secuencia operativa
1. El usuario abre `/login`.
2. El usuario selecciona `¿Olvidaste tu contraseña?`.
3. El sistema muestra la pantalla `/auth/recuperar-password`.
4. La pantalla informa que no existe envío automático por correo.
5. El usuario solicita apoyo a un administrador autorizado.
6. El administrador entra a `/admin/usuarios`.
7. El administrador localiza al usuario y usa la acción `Generar contraseña temporal`.
8. El sistema genera una contraseña temporal y la muestra una sola vez en pantalla.
9. El administrador comparte esa contraseña por un canal seguro.
10. El usuario inicia sesión con la contraseña temporal.
11. El sistema redirige a `/auth/cambiar-password-inicial`.
12. El usuario captura su nueva contraseña y confirma el cambio.
13. El sistema actualiza la contraseña y permite continuar con la sesión normal.

## Reglas del flujo
- La contraseña temporal debe compartirse por un canal seguro.
- La contraseña temporal deja al usuario con `RequiereCambioPassword = true`.
- Al generar contraseña temporal se limpian bloqueos e intentos fallidos del usuario.
- Si el usuario no está activo, no debe generarse contraseña temporal.
- Si más adelante se habilita correo, este documento debe actualizarse para reflejar el nuevo canal de entrega.

## Pantallas involucradas
- `/login`
- `/auth/recuperar-password`
- `/auth/cambiar-password-inicial`
- `/admin/usuarios`

## Responsables
- Usuario final: solicitar recuperación de acceso.
- Administrador: generar y comunicar la contraseña temporal.
- Sistema: obligar el cambio de contraseña después del ingreso temporal.
