<template>
	<span :style="style">{{ sign }}{{ percent }}%</span>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { Prop } from 'vue-property-decorator';

@Options({
	name: 'PercentDiff',
})
export default class MoneyDiff extends Vue {
	@Prop()
	old!: number;

	@Prop()
	new!: number;

	get priceValue() {
		return this.new - this.old;
	}

	get percent() {
		const ratio = this.new / this.old - 1;
		const signPercent = ratio * 100;
		return Math.abs(signPercent).toFixed(2);
	}

	get sign() {
		return this.priceValue > 0 ? '+' : '-';
	}

	get color() {
		return this.priceValue >= 0 ? 'green' : 'red';
	}

	get style() {
		return `color: ${this.color}`;
	}
}
</script>
