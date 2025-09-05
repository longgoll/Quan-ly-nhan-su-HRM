import React from 'react';
import { Link } from 'react-router-dom';
import { Button } from '@/components/ui/button';
import { Home, ArrowLeft } from 'lucide-react';

const NotFoundPage: React.FC = () => {
  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50">
      <div className="text-center">
        <div className="mb-8">
          <h1 className="text-9xl font-bold text-gray-300">404</h1>
          <h2 className="text-2xl font-semibold text-gray-700 mt-4">
            Trang không tìm thấy
          </h2>
          <p className="text-gray-500 mt-2">
            Trang bạn đang tìm kiếm không tồn tại hoặc đã được di chuyển.
          </p>
        </div>
        <div className="space-x-4">
          <Button asChild variant="default">
            <Link to="/dashboard" className="flex items-center gap-2">
              <Home className="h-4 w-4" />
              Về Dashboard
            </Link>
          </Button>
          <Button asChild variant="outline">
            <Link to="/" className="flex items-center gap-2">
              <ArrowLeft className="h-4 w-4" />
              Quay lại
            </Link>
          </Button>
        </div>
      </div>
    </div>
  );
};

export default NotFoundPage;
