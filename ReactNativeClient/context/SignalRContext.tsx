
import React, { createContext, PropsWithChildren, useContext, useEffect, useRef, useState } from 'react';
import { SignalRService } from '@/services/signalRService';
import { User, useSession } from './AuthContext';

interface SignalRContextType {
  signalRService: SignalRService | null;
  isConnected: boolean;
  onlineUsers: User[];
}

const SignalRContext = createContext<SignalRContextType>({
  signalRService: null,
  isConnected: false,
  onlineUsers: [],
});

export function SignalRProvider({ children }: PropsWithChildren) {
  const { userSession } = useSession();
  const [isConnected, setIsConnected] = useState(false);
  const [onlineUsers, setOnlineUsers] = useState<User[]>([]);
  const signalRServiceRef = useRef<SignalRService | null>(null);

  useEffect(() => {
    if (!userSession?.token) return;

    const initializeSignalR = async () => {
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
        
        await signalRServiceRef.current.connect();
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
      }
    };
  }, [userSession?.token]);

  return (
    <SignalRContext.Provider value={{ signalRService: signalRServiceRef.current, isConnected, onlineUsers }}>
      {children}
    </SignalRContext.Provider>
  );
};

export const useSignalR = () => useContext(SignalRContext);