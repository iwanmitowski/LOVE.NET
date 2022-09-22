import styles from "./Footer.module.css";

export default function Footer() {
  const year = new Date().getFullYear();

  return (
    <div className={styles.footer}>
      <div className="p-3">
        © {year} Copyright <strong>Iwan Mitowski</strong>
      </div>
    </div>
  );
}
