import { baseUrl } from '@/apis/base';
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
export interface ChatUser{
  id:string;
  email:string;
  username:string;  
}
export interface Message {
  id: string;
  content: string;
  applicationUser: ChatUser;
  created: Date;
  updated: Date;
}

export interface ChatRoom {
  id: string;
  name: string;
  lastMessage?: {
    content: string;
    created: Date;
    updated: Date;
    username: string;
  };
  messages: Message[];
  users: ChatUser[];
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
  
  public async disconnect(): Promise<void> {
    await this.connection.stop();
  }

  public connectedUserHandler(callback: (message:string) => void): void {
    this.connection.on('ConnectedUserHandler', callback);
  }

  public onlineUsersHandler(callback: (user:ChatUser[]) => void): void {
    this.connection.on('OnlineUsersHandler', callback);
  }

  public userDisconnectedHandler(callback: (user:ChatUser[]) => void): void {
    this.connection.on('UserDisconnectedHandler', callback);
  }

  public createRoomHandler(callback: (roomId: string) => void): void {
    this.connection.on('CreateRoomHandler', callback);
  }

  public getChatRoomsHandler(callback: (rooms: ChatRoom[]) => void): void {
    this.connection.on('GetChatRoomsHandler', callback);
  }

  public joinRoomHandler(callback: (message: string) => void): void {
    this.connection.on('JoinRoomHandler', callback);
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

  public async deleteRoom(roomId: string): Promise<void>{
    try {
      await this.connection.invoke('DeleteRoom', roomId);
    } catch (err) {
      console.error('Error deleting room:', err);
      throw err;
    }
  }

  public sendMessageToRoomHandler (callback: (request: CreateMessageResponse) => void ): void {
    this.connection.on('SendMessageToRoomHandler', callback);
  }

  public async sendMessageToRoom(request: CreateMessageRequest): Promise<void> {
    try {
      await this.connection.invoke('SendMessageToRoom', request);
    } catch (err) {
      console.error('Error sending message:', err);
      throw err;
    }
  }


}
