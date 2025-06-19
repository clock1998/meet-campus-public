
import React, { createContext, PropsWithChildren, useContext, useEffect, useRef, useState } from 'react';
import { ChatRoom, SignalRService } from '@/services/signalRService';
import { User, useSession } from './AuthContext';

interface SignalRContextType {
  signalRService: SignalRService | null;
  isConnected: boolean;
  onlineUsers: User[];
  chatRooms: ChatRoom[];
}

const SignalRContext = createContext<SignalRContextType>({
  signalRService: null,
  isConnected: false,
  onlineUsers: [],
  chatRooms:[]
});

export function SignalRProvider({ children }: PropsWithChildren) {
  const { userSession } = useSession();
  const [isConnected, setIsConnected] = useState(false);
  const [onlineUsers, setOnlineUsers] = useState<User[]>([]);
  const [chatRooms, setChatRooms] = useState<ChatRoom[]>([]);
  const signalRServiceRef = useRef<SignalRService | null>(null);

  useEffect(() => {
    if (!userSession?.token) return;

    try {
      signalRServiceRef.current = new SignalRService(userSession.token);
      signalRServiceRef.current.connectedUserHandler((message: string) => {
        console.log('User connected:', message);
      });
  
      signalRServiceRef.current.OnlineUsersHandler((users: User[]) => {
        setOnlineUsers(users);
      });
        
      signalRServiceRef.current.userDisconnectedHandler((users: User[]) => {
        setOnlineUsers(users);
      });
      
      signalRServiceRef.current.getChatRoomsHandler((rooms: ChatRoom[]) => {
        setChatRooms(rooms);
      });

      signalRServiceRef.current.connect().then(()=>{
        setIsConnected(true);
      });
      
    } catch (error) {
      console.error('Failed to initialize SignalR:', error);
      setIsConnected(false);
    }

    return () => {
      if (signalRServiceRef.current) {
        signalRServiceRef.current.disconnect();
        setIsConnected(false);
      }
    };
  }, [userSession?.token]);

  return (
    <SignalRContext.Provider value={{ signalRService: signalRServiceRef.current, isConnected, onlineUsers, chatRooms }}>
      {children}
    </SignalRContext.Provider>
  );
};

export const useSignalR = () => useContext(SignalRContext);