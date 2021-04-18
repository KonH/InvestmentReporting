<template>
	<span :style="style">{{ sign }}<money :value="priceValueRound" :currency-code="currencyCode" /> ({{ percent }}%)</span>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { Prop } from 'vue-property-decorator';
import Money from '@/component/money.vue';

@Options({
	name: 'MoneyDiff',
	components: {
		Money,
	},
})
export default class MoneyDiff extends Vue {
	@Prop()
	old!: number;

	@Prop()
	new!: number;

	@Prop()
	currencyCode!: string;

	get priceValue() {
		return this.new - this.old;
	}

	get priceValueRound() {
		return this.priceValue.toFixed(2);
	}

	get percent() {
		const ratio = this.new / this.old - 1;
		const signPercent = ratio * 100;
		return Math.abs(signPercent).toFixed(2);
	}

	get sign() {
		return this.priceValue > 0 ? '+' : '';
	}

	get color() {
		return this.priceValue >= 0 ? 'green' : 'red';
	}

	get style() {
		return `color: ${this.color}`;
	}
}
</script>
