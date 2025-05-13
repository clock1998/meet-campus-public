import { User, useSession } from '@/context/AuthContext';
import { SignalRService } from '@/services/signalRService';
import React, { useEffect, useState, useRef } from 'react';
import { View, Text, StyleSheet, FlatList, Alert, ActivityIndicator, TouchableOpacity } from 'react-native';
import { useRouter } from 'expo-router';

export default function ChatScreen() {
  const { userSession } = useSession();
  const [connectedUsers, setConnectedUsers] = useState<User[]>([]);
  const [isConnecting, setIsConnecting] = useState(false);
  const signalRServiceRef = useRef<SignalRService | null>(null);
  const router = useRouter();

  useEffect(() => {
    if (!userSession?.token) return;

    try {
      setIsConnecting(true);
      signalRServiceRef.current = new SignalRService(userSession.token);
      
      // Set up user connection handlers
      signalRServiceRef.current.onUserConnected((message: string) => {
        console.log('User connected:', message);
      });

      signalRServiceRef.current.getOnlineUsers((users: User[]) => {
        setConnectedUsers(users);
      });
        
      signalRServiceRef.current.onUserDisconnected((users: User[]) => {
        setConnectedUsers(users);
      });

      // Set up room creation handler
      signalRServiceRef.current.createRoomHandler((roomId: string) => {
        console.log('Room created:', roomId);
        router.push(`/chat/${roomId}`);
      });

      // Connect to SignalR
      signalRServiceRef.current.connect();
    } catch (error) {
      console.error('Failed to initialize SignalR:', error);
      Alert.alert('Connection Error', 'Failed to connect to chat server');
    } finally {
      setIsConnecting(false);
    }

    // Cleanup on unmount
    return () => {
      if (signalRServiceRef.current) {
        signalRServiceRef.current.disconnect();
      }
    };
  }, [userSession?.token, router]);

  const handleUserPress = async (selectedUser: User) => {
    if (!signalRServiceRef.current || !userSession?.user) return;

    try {
      await signalRServiceRef.current.createRoom({
        userIds: [userSession.user.id, selectedUser.id]
      });
    } catch (error) {
      console.error('Failed to create room:', error);
      Alert.alert('Error', 'Failed to create chat room');
    }
  };

  if (isConnecting) {
    return (
      <View style={styles.loadingContainer}>
        <ActivityIndicator size="large" color="#0000ff" />
        <Text style={styles.loadingText}>Connecting to chat server...</Text>
      </View>
    );
  }

  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <Text style={styles.title}>Online Users</Text>
      </View>

      <FlatList
        style={styles.userList}
        data={connectedUsers}
        keyExtractor={(item) => item.email}
        renderItem={({ item }) => (
          <TouchableOpacity 
            style={styles.userContainer}
            onPress={() => handleUserPress(item)}
          >
            <Text style={styles.username}>{item.email}</Text>
            <View style={styles.onlineIndicator} />
          </TouchableOpacity>
        )}
        ListEmptyComponent={
          <View style={styles.emptyContainer}>
            <Text style={styles.emptyText}>No users online</Text>
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
  userList: {
    flex: 1,
    padding: 10,
  },
  userContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    padding: 15,
    backgroundColor: '#f8f8f8',
    borderRadius: 10,
    marginBottom: 8,
  },
  username: {
    fontSize: 16,
    fontWeight: '500',
    flex: 1,
  },
  onlineIndicator: {
    width: 10,
    height: 10,
    borderRadius: 5,
    backgroundColor: '#4CAF50',
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
  }
});