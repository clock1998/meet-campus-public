 import { User, useSession } from '@/context/AuthContext';
import { SignalRService } from '@/services/signalRService';
import React, { useEffect, useState, useRef } from 'react';
import { View, Text, StyleSheet, TouchableOpacity, ActivityIndicator } from 'react-native';
import { useRouter } from 'expo-router';
import { FlashList } from '@shopify/flash-list';

interface ChatRoom {
  id: string;
  name: string;
  lastMessage?: {
    content: string;
    timestamp: Date;
    senderName: string;
  };
  participants: User[];
}

export default function ChatRoomsScreen() {
  const { userSession } = useSession();
  const [chatRooms, setChatRooms] = useState<ChatRoom[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const signalRServiceRef = useRef<SignalRService | null>(null);
  const router = useRouter();

  useEffect(() => {
    if (!userSession?.token) return;

    try {
      setIsLoading(true);
      signalRServiceRef.current = new SignalRService(userSession.token);

      // Set up room list handler
      signalRServiceRef.current.getUserRoomsHandler((rooms: ChatRoom[]) => {
        setChatRooms(rooms);
      });

      // Set up new room handler
      signalRServiceRef.current.createRoomHandler((roomId: string) => {
        // Refresh room list when a new room is created
        signalRServiceRef.current?.getUserRooms();
      });

      // Connect to SignalR
      signalRServiceRef.current.connect().then(() => {
        // Get initial room list
        signalRServiceRef.current?.getUserRooms();
      });
    } catch (error) {
      console.error('Failed to initialize SignalR:', error);
    } finally {
      setIsLoading(false);
    }

    return () => {
      if (signalRServiceRef.current) {
        signalRServiceRef.current.disconnect();
      }
    };
  }, [userSession?.token]);

  const handleRoomPress = (roomId: string) => {
    router.push({
      pathname: '/(app)/chat/[id]',
      params: { id: roomId }
    });
  };

  if (isLoading) {
    return (
      <View style={styles.loadingContainer}>
        <ActivityIndicator size="large" color="#0000ff" />
        <Text style={styles.loadingText}>Loading chat rooms...</Text>
      </View>
    );
  }

  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <Text style={styles.title}>Chat Rooms</Text>
      </View>

      <FlashList
        estimatedItemSize={80}
        style={styles.roomList}
        data={chatRooms}
        keyExtractor={(item: ChatRoom) => item.id}
        renderItem={({ item }: { item: ChatRoom }) => (
          <TouchableOpacity 
            style={styles.roomContainer}
            onPress={() => handleRoomPress(item.id)}
          >
            <View style={styles.roomInfo}>
              <Text style={styles.roomName}>{item.name}</Text>
              {item.lastMessage && (
                <Text style={styles.lastMessage} numberOfLines={1}>
                  {item.lastMessage.senderName}: {item.lastMessage.content}
                </Text>
              )}
            </View>
            {item.lastMessage && (
              <Text style={styles.timestamp}>
                {new Date(item.lastMessage.timestamp).toLocaleTimeString()}
              </Text>
            )}
          </TouchableOpacity>
        )}
        ListEmptyComponent={
          <View style={styles.emptyContainer}>
            <Text style={styles.emptyText}>No chat rooms yet</Text>
          </View>
        }
      />
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#fff',
  },
  loadingContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: '#fff',
  },
  loadingText: {
    marginTop: 10,
    fontSize: 16,
    color: '#666',
  },
  header: {
    padding: 20,
    borderBottomWidth: 1,
    borderBottomColor: '#eee',
  },
  title: {
    fontSize: 24,
    fontWeight: 'bold',
  },
  roomList: {
    flex: 1,
    padding: 10,
  },
  roomContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    padding: 15,
    backgroundColor: '#f8f8f8',
    borderRadius: 10,
    marginBottom: 8,
  },
  roomInfo: {
    flex: 1,
  },
  roomName: {
    fontSize: 16,
    fontWeight: '500',
    marginBottom: 4,
  },
  lastMessage: {
    fontSize: 14,
    color: '#666',
  },
  timestamp: {
    fontSize: 12,
    color: '#999',
    marginLeft: 10,
  },
  emptyContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    padding: 20,
  },
  emptyText: {
    fontSize: 16,
    color: '#666',
  },
});