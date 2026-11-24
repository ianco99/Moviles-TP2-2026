Patrones de diseño:

1. Strategy.

Utilizado en el arma, maneja las diferencias de disparo para cada modelo. Valores como cantidad de balas, dirección de disparo, cantidad de balas por disparo, entre otras, son manejadas gracias a este patrón.
(IFireStrategy.cs, FireStrategy.cs, Gun.cs, SingleShot.cs, SpreadShot.cs)


2. Command

El patrón command es usado para la navegación de menus y reutilización de acciones varias como pausar el juego, reiniciar una escena o cargar un menú.

(ICommand.cs, CommandInvoker.cs, FireCommand.cs)


3. Memento

Guarda el progreso del jugador en las diferentes areas. El dinero que tiene el jugador, Las armas que tiene, las skins.

(WalletSnapshot, InventorySnapshot, EconomySnapchots.cs)

4. Facade

Un servicio (ServiceProvider) enmascara todas las funcionalidades que se relacionan directamente con un dispositivo android. Vibración y Plugin son manejados a través de esta clase, que expone métodos públicos para su llamado a lo largo de la aplicación.

(MobilePLatform.cs)

5. Bootstrapper

6. EventBus

7. ObjectPool

8. Service
