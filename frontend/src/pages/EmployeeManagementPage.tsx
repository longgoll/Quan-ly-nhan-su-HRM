import React from 'react';
import EmployeeList from '@/components/employees/EmployeeList';

const EmployeeManagementPage: React.FC = () => {
  return (
    <div className="container mx-auto p-6">
      <EmployeeList />
    </div>
  );
};

export default EmployeeManagementPage;
