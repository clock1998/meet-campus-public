import React, { createContext, PropsWithChildren, useContext, useEffect, useRef, useState } from 'react';
import { ChatUser, ChatRoom, SignalRService } from '@/services/signalRService';
import { useSession } from './AuthContext';
import { router } from 'expo-router';

interface SignalRContextType {
  signalRService: SignalRService | null;
  isConnected: boolean;
  onlineUsers: ChatUser[];
  chatRooms: ChatRoom[];
}

const SignalRContext = createContext<SignalRContextType>({
  signalRService: null,
  isConnected: false,
  onlineUsers: [],
  chatRooms: [],
});

export function SignalRProvider({ children }: PropsWithChildren) {
  const { userSession } = useSession();
  const [isConnected, setIsConnected] = useState(false);
  const [onlineUsers, setOnlineUsers] = useState<ChatUser[]>([]);
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
    
        service.onlineUsersHandler((users: ChatUser[]) => {
          // console.log(users)
          setOnlineUsers(users);
        });
          
        service.userDisconnectedHandler((users: ChatUser[]) => {
          setOnlineUsers([...users]);
        });
        
        service.createRoomHandler((room: ChatRoom) => {
          setChatRooms(prev => [...prev,room]);
        });

        service.getChatRoomsHandler((rooms: ChatRoom[]) => {
          setChatRooms(rooms);
        });
        
        service.createRoomHandler((room: ChatRoom) => {
          console.log('Room created:', room.id);
          router.push({
            pathname: '/(app)/chat/[id]',
            params: { id: room.id }
          });
        });

        service.deleteRoomHandler((deletedRoomId: string) => {
          setChatRooms(prevRooms => prevRooms.filter(room => room.id !== deletedRoomId));
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