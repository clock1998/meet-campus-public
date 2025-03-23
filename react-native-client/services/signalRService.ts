import { baseUrl } from '@/apis/base';
import { HubConnectionBuilder, LogLevel, HubConnection } from '@microsoft/signalr';

export interface CreateMessageRequest {
  userId: string;
  roomId: string;
  content: string;
}

export class SignalRService {
  private connection: HubConnection | null = null;
  private token: string;

  constructor(token: string) {
    this.token = token;
  }

  public connect(): HubConnection {
    if (!this.connection) {
      this.connection = new HubConnectionBuilder()
        .withUrl(`${baseUrl}/hubs/chat`, {
          accessTokenFactory: () => this.token,
        })
        .configureLogging(LogLevel.Information)
        .withAutomaticReconnect()
        .build();
      
      this.connection
      .start()
      .then(()=>{console.log('Connected to ChatHub')})
      .catch((err)=>{console.error('Error connecting to ChatHub:', err);})
    }
    return this.connection;
  }

  public onUserConnected(callback: (userId: string, username: string) => void): void {
    this.connection?.on('UserConnectedHandler', callback);
  }

  public createRoom(roomId: string): void {
    this.connection
      ?.invoke('CreateRoomHandler', roomId)
      .catch(err => console.error(err));
  }

  public sendMessageToRoom(request: CreateMessageRequest): void {
    this.connection
      ?.invoke('SendMessageToRoomHandler', request)
      .catch(err => console.error(err));
  }
}
