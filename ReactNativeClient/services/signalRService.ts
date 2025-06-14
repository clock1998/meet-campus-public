import { baseUrl } from '@/apis/base';
import { User } from '@/context/AuthContext';
import { HubConnectionBuilder, LogLevel, HubConnection } from '@microsoft/signalr';

export interface CreateMessageRequest {
  userId: string;
  roomId: string;
  content: string;
}

export interface CreateMessageResponse{
  id: string;
  content: string;
  username: string;
  created: Date;
  updated: Date;
}

export interface CreateRoomRequest {
  userIds: string[];
}

export interface ChatRoom {
  id: string;
  name: string;
  lastMessage?: {
    content: string;
    timestamp: Date;
    senderName: string;
  };
  participants: User[];
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

  public connectedUserHandler(callback: (message:string) => void): void {
    this.connection.on('ConnectedUserHandler', callback);
  }

  public OnlineUsersHandler(callback: (user:User[]) => void): void {
    this.connection.on('OnlineUsersHandler', callback);
  }

  public userDisconnectedHandler(callback: (user:User[]) => void): void {
    this.connection.on('UserDisconnectedHandler', callback);
  }

  public createRoomHandler(callback: (roomId: string) => void): void {
    this.connection.on('CreateRoomHandler', callback);
  }

  public getUserRoomsHandler(callback: (rooms: ChatRoom[]) => void): void {
    this.connection.on('GetUserRoomsHandler', callback);
  }

  public async getUserRooms(): Promise<void> {
    try {
      await this.connection.invoke('GetUserRooms');
    } catch (err) {
      console.error('Error getting user rooms:', err);
      throw err;
    }
  }

  public async createRoom(request: CreateRoomRequest): Promise<void> {
    try {
      await this.connection.invoke('CreateRoom', request);
    } catch (err) {
      console.error('Error creating room:', err);
      throw err;
    }
  }

  public async joinRoom(roomId: string): Promise<void> {
    try {
      await this.connection.invoke('JoinRoom', roomId);
    } catch (err) {
      console.error('Error creating room:', err);
      throw err;
    }
  }

  public sendMessageToRoomHandler (callback: (request: CreateMessageResponse) => void ): void {
    this.connection.on('SendMessageToRoomHandler', callback);
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
