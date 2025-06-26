import React, { createContext, PropsWithChildren, useContext, useEffect, useRef, useState } from 'react';
import { CahtUser, ChatRoom, SignalRService } from '@/services/signalRService';
import { useSession } from './AuthContext';

interface SignalRContextType {
  signalRService: SignalRService | null;
  isConnected: boolean;
  onlineUsers: CahtUser[];
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
  const [onlineUsers, setOnlineUsers] = useState<CahtUser[]>([]);
  const [chatRooms, setChatRooms] = useState<ChatRoom[]>([]);
  const signalRServiceRef = useRef<SignalRService | null>(null);

  useEffect(() => {
    if (!userSession?.token) return;

    const initializeSignalR = async () => {
      try {
        const service = new SignalRService(userSession.token);
        signalRServiceRef.current = service;

        service.connectedUserHandler((message: string) => {
          console.log('User connected:', message);
        });
    
        service.OnlineUsersHandler((users: CahtUser[]) => {
          console.log(users)
          setOnlineUsers(users);
        });
          
        service.userDisconnectedHandler((users: CahtUser[]) => {
          setOnlineUsers(users);
        });
        
        service.getChatRoomsHandler((rooms: ChatRoom[]) => {
          console.log("Updating Chatrooms:", rooms);
          setChatRooms(rooms);
        });
  
        await service.connect();
        setIsConnected(true);
        
      } catch (error) {
        console.error('Failed to initialize SignalR:', error);
        setIsConnected(false);
      }
    };

    initializeSignalR();

    return () => {
      if (signalRServiceRef.current) {
        signalRServiceRef.current.disconnect();
        setIsConnected(false);
        signalRServiceRef.current = null;
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