import { useState } from 'react';
import { FunctionTester } from './components/FunctionTester';
import { WorkbookEditor } from './components/WorkbookEditor';

type TabType = 'workbook' | 'function';

function App() {
  const [activeTab, setActiveTab] = useState<TabType>('workbook');

  return (
    <div>
      <nav style={styles.nav}>
        <button
          style={{
            ...styles.tab,
            ...(activeTab === 'workbook' ? styles.activeTab : {}),
          }}
          onClick={() => setActiveTab('workbook')}
        >
          Workbook Editor
        </button>
        <button
          style={{
            ...styles.tab,
            ...(activeTab === 'function' ? styles.activeTab : {}),
          }}
          onClick={() => setActiveTab('function')}
        >
          Function Tester
        </button>
      </nav>

      {activeTab === 'workbook' && <WorkbookEditor />}
      {activeTab === 'function' && <FunctionTester />}
    </div>
  );
}

const styles: Record<string, React.CSSProperties> = {
  nav: {
    display: 'flex',
    gap: '4px',
    padding: '16px 20px',
    backgroundColor: '#343a40',
  },
  tab: {
    padding: '10px 20px',
    fontSize: '14px',
    border: 'none',
    borderRadius: '4px 4px 0 0',
    backgroundColor: '#495057',
    color: '#fff',
    cursor: 'pointer',
  },
  activeTab: {
    backgroundColor: '#007bff',
  },
};

export default App;
