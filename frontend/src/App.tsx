import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { AuthProvider } from './contexts/AuthContext';
import Header from './components/Header';
import QsoManagerPagePaginated from './components/QsoManagerPagePaginated';
import QsoDetailPage from './components/QsoDetailPage';
import QsoEditPage from './components/QsoEditPage';
import LoginPage from './components/LoginPage';
import RegisterPage from './components/RegisterPage';
import ProfilePage from './components/ProfilePage';
import ProtectedRoute from './components/ProtectedRoute';
import ErrorTestComponent from './components/ErrorTestComponent';
import './styles/global.css';

const App: React.FC = () => {
  return (
    <AuthProvider>
      <Router>
        <div className="app">
          <Header />
          <main className="main-content">            <Routes>
              <Route path="/" element={<QsoManagerPagePaginated />} />
              <Route path="/qso/:id" element={<QsoDetailPage />} />
              <Route path="/qso/:id/edit" element={<QsoEditPage />} />
              <Route path="/login" element={<LoginPage />} />
              <Route path="/register" element={<RegisterPage />} />
              <Route path="/profile" element={
                <ProtectedRoute>
                  <ProfilePage />
                </ProtectedRoute>
              } />
              <Route path="/test-errors" element={<ErrorTestComponent />} />
            </Routes>
          </main>
        </div>
      </Router>
    </AuthProvider>
  );
};

export default App;
