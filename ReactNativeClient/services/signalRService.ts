import { baseUrl } from '@/apis/base';
import { User } from '@/context/AuthContext';
import { HubConnectionBuilder, LogLevel, HubConnection } from '@microsoft/signalr';

export interface CreateMessageRequest {
  userId: string;
  roomId: string;
  content: string;
}

export interface CreateRoomRequest {
  userIds: string[];
}

export class SignalRService {
  private connection: HubConnection;
  private token: string;

  constructor(token: string) {
    this.token = token;
    this.connection = new HubConnectionBuilder()
      .withUrl(`${baseUrl}/hubs/chat`, {
        accessTokenFactory: () => this.token,
      })
      .configureLogging(LogLevel.Information)
      .withAutomaticReconnect()
      .build();
  }

  public async connect(): Promise<HubConnection> {
    try {
      await this.connection.start();
      console.log('Connected to ChatHub');
      return this.connection;
    } catch (err) {
      console.error('Error connecting to ChatHub:', err);
      throw err;
    }
  }

  public onUserConnected(callback: (message:string) => void): void {
    this.connection.on('UserConnectedHandler', callback);
  }
  public getOnlineUsers(callback: (user:User[]) => void): void {
    this.connection.on('UsersConnectedHandler', callback);
  }
  public onUserDisconnected(callback: (user:User[]) => void): void {
    this.connection.on('UserDisconnectedHandler', callback);
  }

  public createRoomHandler(callback: (message: string) => void): void {
    this.connection.on('CreateRoomHandler', callback);
  }

  public async createRoom(request: CreateRoomRequest): Promise<void> {
    try {
      await this.connection.invoke('CreateRoom', request);
    } catch (err) {
      console.error('Error creating room:', err);
      throw err;
    }
  }

  public async sendMessageToRoom(request: CreateMessageRequest): Promise<void> {
    try {
      await this.connection.invoke('SendMessageToRoomHandler', request);
    } catch (err) {
      console.error('Error sending message:', err);
      throw err;
    }
  }

  public async disconnect(): Promise<void> {
    await this.connection.stop();
  }
}
